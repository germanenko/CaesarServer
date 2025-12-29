using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;

namespace planner_node_service.Infrastructure.Repository
{
    public class NodeRepository : INodeRepository
    {
        private readonly NodeDbContext _context;

        public NodeRepository(NodeDbContext context)
        {
            _context = context;
        }

        public async Task<Node> AddOrUpdateNode(Node newNode)
        {
            var existingNode = await _context.Nodes
                .Where(x => x.Id == newNode.Id)
                .FirstOrDefaultAsync();

            if (existingNode == null)
            {
                var result = await _context.Nodes.AddAsync(newNode);

                await AddOrUpdateNodeLink(new NodeLinkBody()
                {
                    ParentId = newNode.Id,
                    ChildId = newNode.Id,
                    RelationType = RelationType.Me
                });

                await _context.SaveChangesAsync();
                return result.Entity;
            }
            else
            {
                _context.Entry(existingNode).CurrentValues.SetValues(newNode);
                await _context.SaveChangesAsync();
                return existingNode;
            }
        }

        public async Task<NodeLink> AddOrUpdateNodeLink(NodeLinkBody newNodeLink)
        {
            var existingNode = await _context.NodeLinks
                .Where(x => x.Id == newNodeLink.Id)
                .FirstOrDefaultAsync();

            if (existingNode == null)
            {
                var newLink = new NodeLink
                {
                    Id = newNodeLink.Id,
                    ParentId = newNodeLink.ParentId,
                    ChildId = newNodeLink.ChildId,
                    RelationType = newNodeLink.RelationType
                };
                var result = await _context.NodeLinks.AddAsync(newLink);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            else
            {
                existingNode.ParentId = newNodeLink.ParentId;
                existingNode.ChildId = newNodeLink.ChildId;
                existingNode.RelationType = newNodeLink.RelationType;
                await _context.SaveChangesAsync();
                return existingNode;
            }
        }

        public async Task<List<NodeLink>> AddOrUpdateNodeLinks(List<NodeLinkBody> newNodeLinks)
        {
            var links = new List<NodeLink>();

            foreach (var link in newNodeLinks)
            {
                links.Add(await AddOrUpdateNodeLink(link));
            }

            return links;
        }

        public async Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null)
        {
            var query = _context.NodeLinks.Where(x => x.ParentId == parentId);

            if (relationType.HasValue)
            {
                query = query.Where(x => x.RelationType == relationType.Value);
            }

            return await query.Select(x => x.ChildId).ToListAsync();
        }

        public async Task<IEnumerable<Node>?> GetNodes(Guid accountId)
        {
            var links = await GetNodeLinks(accountId);
            if (links == null || !links.Any())
                return Enumerable.Empty<Node>();

            var nodeIds = links
                .SelectMany(x => new[] { x.ParentId, x.ChildId })
                .Distinct()
                .ToList();

            return await _context.Nodes
                .Where(x => nodeIds.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<NodeLink>?> GetNodeLinks(Guid accountId)
        {
            var rootIds = await _context.AccessRights
                .Where(x => x.AccountId == accountId)
                .Select(x => x.NodeId)
                .ToListAsync();

            if (!rootIds.Any())
                return Enumerable.Empty<NodeLink>();

            var query = @"
                WITH RECURSIVE node_tree AS (
                    SELECT 
                        nl.*, 
                        1 as level,
                        ARRAY[nl.""ParentId""] as visited_path 
                    FROM ""NodeLinks"" nl 
                    WHERE nl.""ParentId"" IN ({0})
                    
                    UNION ALL
                    
                    SELECT 
                        n.*, 
                        nt.level + 1,
                        nt.visited_path || n.""ParentId"" 
                    FROM ""NodeLinks"" n
                    INNER JOIN node_tree nt ON n.""ParentId"" = nt.""ChildId""
                    WHERE 
                        nt.level < 5
                        AND n.""ParentId"" != n.""ChildId""
                        AND n.""ParentId"" != ALL(nt.visited_path) 
                )
                SELECT ""Id"", ""ParentId"", ""ChildId"", ""RelationType"" 
                FROM node_tree 
                WHERE level <= 5
                LIMIT 500;";

            var parameters = rootIds.Select((id, i) =>
                new NpgsqlParameter($"@p{i}", id)).ToArray();

            var paramNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return await _context.NodeLinks
                .FromSqlRaw(string.Format(query, paramNames), parameters)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}