using Microsoft.EntityFrameworkCore;
using Npgsql;
using Planer_task_board.Core.Entities.Models;
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

        public async Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null)
        {
            var query = _context.Nodes.Where(x => x.ParentId == parentId);

            if (relationType.HasValue)
            {
                query = query.Where(x => x.RelationType == relationType.Value);
            }

            return await query.Select(x => x.ChildId).ToListAsync();
        }

        public async Task<IEnumerable<Node>?> GetNodes(Guid accountId)
        {
            var accessibleResourceIds = await _context.AccessRights
                .Where(x => x.AccountId == accountId)
                .Select(x => x.ResourceId)
                .ToListAsync();

            if (!accessibleResourceIds.Any())
                return Enumerable.Empty<Node>();

            var allNodes = await _context.Nodes.ToListAsync();
            var resultNodes = new HashSet<Node>();
            var nodesToProcess = new Queue<Guid>(accessibleResourceIds);

            while (nodesToProcess.Count > 0)
            {
                var currentNodeId = nodesToProcess.Dequeue();
                var currentNode = allNodes.FirstOrDefault(x => x.Id == currentNodeId);

                if (currentNode != null && resultNodes.Add(currentNode))
                {
                    var childrenIds = allNodes
                        .Where(x => x.ParentId == currentNodeId)
                        .Select(x => x.Id);

                    foreach (var childId in childrenIds)
                    {
                        nodesToProcess.Enqueue(childId);
                    }
                }
            }

            return resultNodes.ToList();

            //var accessibleResourceIds = await _context.AccessRights
            //    .Where(x => x.AccountId == accountId)
            //    .Select(x => x.ResourceId)
            //    .ToListAsync();

            //var nodes = await _context.Nodes
            //.Where(x => accessibleResourceIds.Contains(x.ParentId) ||
            //       accessibleResourceIds.Contains(x.Id))
            //.ToListAsync();

            //var nodes = await _context.Nodes.Where(x => accessibleResourceIds.Contains(x.ParentId)).ToListAsync();

            //return nodes;
        }
    }
}