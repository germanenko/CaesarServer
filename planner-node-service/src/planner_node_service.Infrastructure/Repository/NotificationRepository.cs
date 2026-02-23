using Microsoft.EntityFrameworkCore;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
namespace planner_node_service.Infrastructure.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NodeDbContext _context;

        public NotificationRepository(NodeDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationSettings>> GetEnabledNotificationSettingsAsync(List<Guid> accountIds)
        {
            return await _context.NotificationSettings.Where(x => accountIds.Contains(x.AccountId) && x.NotificationsEnabled).ToListAsync();
        }
    }
}
