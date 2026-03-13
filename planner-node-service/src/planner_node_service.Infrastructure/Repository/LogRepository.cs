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

        public async Task<History?> AddHistory(History history)
        {
            var hasHistory = _context.History.FirstOrDefault(x => x.TrackableId == history.TrackableId);
            var hasLog = _context.ContentLogs.FirstOrDefault(x => x.EntityId == history.TrackableId);

            history.Action = hasHistory == null ? ActionType.Create : ActionType.Update;

            var log = new ContentLog(history.TrackableId, history.TrackableId, hasHistory == null ? ActionType.Create : ActionType.Update);

            var result = (await _context.History.AddAsync(history)).Entity;

            await _context.SaveChangesAsync();

            await AddContentLog(log);

            return result;
        }

        public async Task<ContentLog?> AddContentLog(ContentLog log)
        {
            var hasLog = _context.ContentLogs.FirstOrDefault(x => x.EntityId == log.EntityId);

            log.Action = hasLog == null ? ActionType.Create : ActionType.Update;

            var result = (await _context.ContentLogs.AddAsync(log)).Entity;

            (_context.ContentLogs.FirstOrDefault(x => x.EntityId == log.EntityId)).Entity.CursorId = result.Id;

            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<History?> GetCreateHistory(Guid nodeId)
        {
            var history = await _context.History.SingleOrDefaultAsync(x => x.TrackableId == nodeId && x.Action == ActionType.Create);

            return history;
        }

        public async Task<History?> GetLastHistory(Guid nodeId)
        {
            var history = await _context.History.Where(x => x.TrackableId == nodeId).OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();

            return history;
        }

        public async Task<ContentLog?> GetLastLogForEntity(Guid entityId)
        {
            var log = await _context.ContentLogs.Where(x => x.EntityId == entityId).OrderByDescending(x => x.Version).FirstOrDefaultAsync();

            return log;
        }
    }
}
