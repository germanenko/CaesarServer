using Microsoft.EntityFrameworkCore;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
using planner_server_package.Entities;
namespace planner_node_service.Infrastructure.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NodeDbContext _context;

        public NotificationRepository(NodeDbContext context)
        {
            _context = context;
        }

        // Получить все настройки уведомлений для заданных идентификаторов аккаунтов, где уведомления включены
        public async Task<List<NotificationSettings>> GetEnabledNotificationSettingsAsync(List<Guid> accountIds)
        {
            return await _context.NotificationSettings.Where(x => accountIds.Contains(x.AccountId) && x.NotificationsEnabled).ToListAsync();
        }

        // Добавить новые настройки уведомлений
        public async Task<NotificationSettings> AddNotificationSettings(NotificationSettingsBody notificationSettingsBody)
        {
            var result = (await _context.NotificationSettings.AddAsync(
                new NotificationSettings()
                {
                    AccountId = notificationSettingsBody.AccountId,
                    NodeId = notificationSettingsBody.NodeId,
                    NotificationsEnabled = notificationSettingsBody.NotificationsEnabled
                })).Entity;

            await _context.SaveChangesAsync();

            return result;
        }
    }
}
