using Microsoft.EntityFrameworkCore;
using Npgsql;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
using planner_server_package.Entities;

namespace planner_node_service.Infrastructure.Repository
{
    public class NodeRepository : INodeRepository
    {
        private readonly NodeDbContext _context;

        public NodeRepository(NodeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Node>> AddOrUpdateNodes(List<Node> nodes)
        {
            List<Node> newNodes = new List<Node>();
            foreach (var node in nodes)
            {
                newNodes.Add(await AddOrUpdateNode(node));
            }
            return newNodes;
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
                .Where(x => x.ChildId == newNodeLink.ChildId && x.ParentId == newNodeLink.ParentId)
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
            IEnumerable<Node>? links = await GetNodesTree(accountId);

            return links;

            //if (links == null || !links.Any())
            //    return Enumerable.Empty<Node>();

            //var nodeIds = links
            //    .SelectMany(x => new[] { x.ParentId, x.ChildId })
            //    .Distinct()
            //    .ToList();

            //return await _context.Nodes
            //    .Where(x => nodeIds.Contains(x.Id))
            //    .ToListAsync();
        }


        public async Task<IEnumerable<Node>?> GetNodesTree(Guid accountId)
        {
            var rootIds = await _context.AccessRights
                .Where(x => x.AccountId == accountId)
                .Select(x => x.NodeId)
                .ToListAsync();

            if (!rootIds.Any())
                return Enumerable.Empty<Node>();

            var allNodeIds = new HashSet<Guid>(rootIds);
            var currentLevelIds = new HashSet<Guid>(rootIds);
            var allNodes = new List<Node>();

            for (var level = 0; level < 5 && currentLevelIds.Any(); level++)
            {
                var links = await _context.NodeLinks
                    .Where(x => currentLevelIds.Contains(x.ParentId) && x.ParentId != x.ChildId)
                    .Include(x => x.ParentNode)
                    .Include(x => x.ChildNode)
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var link in links)
                {
                    allNodes.Add(link.ParentNode);
                    allNodes.Add(link.ChildNode);
                }

                var nextLevelIds = new HashSet<Guid>();
                foreach (var link in links)
                {
                    if (allNodeIds.Add(link.ChildId))
                    {
                        nextLevelIds.Add(link.ChildId);
                    }
                }

                currentLevelIds = nextLevelIds;
            }

            return allNodes.DistinctBy(x => x.Id).ToList();
        }

        public async Task<IEnumerable<NodeLink>?> GetNodesLinks(Guid accountId)
        {
            var rootIds = await _context.AccessRights
                .Where(x => x.AccountId == accountId)
                .Select(x => x.NodeId)
                .ToListAsync();

            if (!rootIds.Any())
                return Enumerable.Empty<NodeLink>();

            var allLinks = new List<NodeLink>();
            var visitedNodeIds = new HashSet<Guid>(rootIds);
            var currentLevelIds = new HashSet<Guid>(rootIds);

            for (var level = 0; level < 5 && currentLevelIds.Any(); level++)
            {
                var links = await _context.NodeLinks
                    .Where(x => currentLevelIds.Contains(x.ParentId) && x.ParentId != x.ChildId)
                    .AsNoTracking()
                    .ToListAsync();

                allLinks.AddRange(links);

                var nextLevelIds = new HashSet<Guid>();
                foreach (var link in links)
                {
                    if (visitedNodeIds.Add(link.ChildId))
                    {
                        nextLevelIds.Add(link.ChildId);
                    }
                }

                currentLevelIds = nextLevelIds;
            }

            return allLinks
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();
        }
    }
}