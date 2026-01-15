using CaesarServerLibrary.Enums;

namespace Planner_chat_server.Core.IService
{
    public interface INotificationService
    {
        Task<bool> SendNotification(Guid userId, string title, string content, NotificationType type, Dictionary<string, string>? data = null);
    }
}
