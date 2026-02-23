using planner_client_package.Entities;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using System.Net;
using ServerNotificationSettings = planner_server_package.Entities.NotificationSettingsBody;

namespace planner_node_service.App.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(
            INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<ServiceResponse<List<ServerNotificationSettings>>> GetEnabledNotificationSettings(List<Guid> accountIds)
        {
            var settings = await _notificationRepository.GetEnabledNotificationSettingsAsync(accountIds);

            return new ServiceResponse<List<ServerNotificationSettings>>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = settings.Select(x => new ServerNotificationSettings() { NodeId = x.NodeId, NotificationsEnabled = x.NotificationsEnabled, AccountId = x.AccountId }).ToList()
            };
        }

        public async Task<ServiceResponse<NotificationSettings>> AddNotificationSettings(ServerNotificationSettings notificationSettingsBody)
        {
            var settings = await _notificationRepository.AddNotificationSettings(notificationSettingsBody);

            return new ServiceResponse<NotificationSettings>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = settings
            };
        }
    }
}
