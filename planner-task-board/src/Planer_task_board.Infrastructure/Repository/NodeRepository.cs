using Microsoft.EntityFrameworkCore;
using Npgsql;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Infrastructure.Data;

namespace Planer_task_board.Infrastructure.Repository
{
    public class NodeRepository : INodeRepository
    {
        private readonly ContentDbContext _context;

        public NodeRepository(ContentDbContext context)
        {
            _context = context;
        }

        public async Task<Node> AddOrUpdateNode(Guid accountId, Node newNode)
        {
            var existingNode = await _context.Nodes
                .Where(x => x.Id == newNode.Id)
                .FirstOrDefaultAsync();

            if (existingNode == null)
            {
                var result = await _context.Nodes.AddAsync(newNode);
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

        public async Task<NodeLink> AddOrUpdateNodeLink(Guid accountId, CreateOrUpdateNodeLink newNodeLink)
        {
            var existingNode = await _context.NodeLinks
                .Where(x => x.Id == newNodeLink.Id)
                .FirstOrDefaultAsync();

            if (existingNode == null)
            {
                var newLink = new NodeLink
                {
                    Id = newNodeLink.Id,
                    ParentId = accountId,
                    ChildId = newNodeLink.Id,
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
            //var result = new List<Node>();

            //result.AddRange(await _context.Boards.ToListAsync());
            //result.AddRange(await _context.Columns.ToListAsync());
            //result.AddRange(await _context.Tasks.ToListAsync());

            //return result;

            var links = await GetNodeLinks(accountId);
            if (links == null || !links.Any())
                return null;

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
            var accessibleResourceIds = await _context.AccessRights
                .Where(x => x.AccountId == accountId)
                .Select(x => x.NodeId)
                .ToListAsync();

            if (!accessibleResourceIds.Any())
                return null;

            var query = @"
                WITH RECURSIVE node_tree AS (
                    SELECT nl.* FROM ""NodeLinks"" nl
                    WHERE nl.""ParentId"" IN ({0})
                    UNION ALL
                    SELECT n.* FROM ""NodeLinks"" n
                    INNER JOIN node_tree nt ON n.""ParentId"" = nt.""ChildId""
                )
                SELECT DISTINCT * FROM node_tree";

            var parameters = new List<NpgsqlParameter>();
            var paramNames = new List<string>();

            for (int i = 0; i < accessibleResourceIds.Count; i++)
            {
                var paramName = $"@p{i}";
                paramNames.Add(paramName);
                parameters.Add(new NpgsqlParameter(paramName, accessibleResourceIds[i]));
            }

            var formattedQuery = string.Format(query, string.Join(",", paramNames));

            return await _context.NodeLinks
                .FromSqlRaw(formattedQuery, parameters.ToArray())
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<NodeLink>> GetTreeByRootId(Guid rootId)
        {
            var query = @"
                WITH RECURSIVE node_tree AS (
                    SELECT * FROM ""NodeLinks"" WHERE ""ParentId"" = @rootId
                    UNION ALL
                    SELECT n.* FROM ""NodeLinks"" n
                    INNER JOIN node_tree nt ON n.""ParentId"" = nt.""ChildId""
                )
                SELECT * FROM node_tree";

            return await _context.NodeLinks
                .FromSqlRaw(query, new NpgsqlParameter("@rootId", rootId))
                .ToListAsync();
        }
    }
}