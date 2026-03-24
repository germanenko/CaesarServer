using Microsoft.EntityFrameworkCore;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;

namespace planner_node_service.Infrastructure.Repository
{
    public class LogRepository : ILogRepository
    {
        private readonly NodeDbContext _context;

        public LogRepository(NodeDbContext context)
        {
            _context = context;
        }

        public async Task<History?> GetCreateHistory(Guid nodeId)
        {
            var history = await _context.History.SingleOrDefaultAsync(x => x.NodeId == nodeId && x.Action == ActionType.Create);

            return history;
        }

        public async Task<History?> GetLastHistory(Guid nodeId)
        {
            var history = await _context.History.Where(x => x.NodeId == nodeId).OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();

            return history;
        }

        public async Task<ContentLog?> GetLastLogForEntity(Guid entityId)
        {
            var log = await _context.ContentLogs.Where(x => x.EntityId == entityId).OrderByDescending(x => x.ScopeVersion).FirstOrDefaultAsync();

            return log;
        }

        public async Task<ContentLog?> GetLastLogForScope(Guid scopeId)
        {
            var log = await _context.ContentLogs.Where(x => x.ScopeId == scopeId).OrderByDescending(x => x.Seq).FirstOrDefaultAsync();

            return log;
        }
    }
}
