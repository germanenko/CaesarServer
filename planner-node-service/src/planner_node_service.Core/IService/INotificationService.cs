using planner_client_package.Entities;
using ServerNotificationSettings = planner_server_package.Entities.NotificationSettingsBody;

namespace planner_node_service.Core.IService
{
    public interface INotificationService
    {
        public Task<ServiceResponse<List<ServerNotificationSettings>>> GetEnabledNotificationSettings(List<Guid> accountIds);
    }
}
