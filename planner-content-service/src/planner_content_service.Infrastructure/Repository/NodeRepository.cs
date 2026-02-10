using Microsoft.EntityFrameworkCore;
using Npgsql;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Infrastructure.Data;

namespace planner_content_service.Infrastructure.Repository
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


        public async Task<IEnumerable<Node>?> GetNodesByIds(List<Guid> nodeIds)
        {
            return await _context.Nodes
                .Where(x => nodeIds.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<Node>?> GetNodes(Guid accountId, List<Guid> nodeIds)
        {
            return await _context.Nodes
                .Where(x => nodeIds.Contains(x.Id))
                .ToListAsync();
        }
    }
}