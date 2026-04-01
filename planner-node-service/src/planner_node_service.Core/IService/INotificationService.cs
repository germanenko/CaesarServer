using planner_client_package.Entities;
using planner_node_service.Core.Entities.Models;
using planner_server_package;
using ServerNotificationSettings = planner_server_package.Entities.NotificationSettingsBody;

namespace planner_node_service.Core.IService
{
    public interface INotificationService
    {
        public Task<ServiceResponse<List<ServerNotificationSettings>>> GetEnabledNotificationSettings(List<Guid> accountIds);
        public Task<ServiceResponse<NotificationSettings>> AddNotificationSettings(ServerNotificationSettings notificationSettingsBody);
    }
}
