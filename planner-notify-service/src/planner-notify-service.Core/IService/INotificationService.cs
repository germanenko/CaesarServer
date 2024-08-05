using planner_notify_service.Core.Entities.Response;

namespace planner_notify_service.Core.IService
{
    public interface INotificationService
    {
        NotificationSession AddSession(Guid accountId, NotificationSession session);
        IEnumerable<NotificationSession> GetSessions(Guid accountId);
        bool RemoveSession(Guid accountId, Guid sessionId);
        Task<IEnumerable<Guid>> SendMessageToSessions(Guid accountId, List<Guid> sessionIds, byte[] bytes);
        Task SendMessageToSessions(Guid accountId, byte[] bytes);
    }
}