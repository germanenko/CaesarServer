using planner_common_package.Enums;

namespace planner_chat_service.Core.IService
{
    public interface INotificationService
    {
        Task<bool> SendNotification(Guid userId, string title, string content, NotificationType type, Dictionary<string, string>? data = null);
    }
}
