using Microsoft.EntityFrameworkCore;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
using static NpgsqlTypes.NpgsqlTsQuery;

namespace planner_node_service.Infrastructure.Repository
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly NodeDbContext _context;

        public HistoryRepository(NodeDbContext context)
        {
            _context = context;
        }

        public async Task<History?> AddHistory(History history)
        {
            var haveHistory = _context.History.FirstOrDefault(x => x.TrackableId == history.TrackableId);

            history.Action = haveHistory == null ? ActionType.Create : ActionType.Update;

            var result = (await _context.History.AddAsync(history)).Entity;

            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<History?> GetCreateHistory(Guid nodeId)
        {
            var history = await _context.History.SingleOrDefaultAsync(x => x.TrackableId == nodeId && x.Action == ActionType.Create);

            return history;
        }
    }
}
