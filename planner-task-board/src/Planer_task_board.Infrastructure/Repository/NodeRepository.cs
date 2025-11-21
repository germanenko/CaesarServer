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

            // —оздаем CTE запрос дл€ каждого доступного ресурса
            var allTreesNodes = new List<Node>();

            foreach (var resourceId in accessibleResourceIds)
            {
                var treeNodes = await GetTreeByRootId(resourceId);
                allTreesNodes.AddRange(treeNodes);
            }

            return allTreesNodes.Distinct().ToList();
        }
        private async Task<List<Node>> GetTreeByRootId(Guid rootId)
        {
            var query = @"
                WITH RECURSIVE node_tree AS (
                    SELECT * FROM ""Nodes"" WHERE ""ParentId"" = @rootId
                    UNION ALL
                    SELECT n.* FROM ""Nodes"" n
                    INNER JOIN node_tree nt ON n.""ParentId"" = nt.""ChildId""
                )
                SELECT * FROM node_tree";

            return await _context.Nodes
                .FromSqlRaw(query, new NpgsqlParameter("@rootId", rootId))
                .ToListAsync();
        }
    }
}