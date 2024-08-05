using planner_notify_service.Core.Entities.Response;

namespace planner_notify_service.Core.IService
{
    public interface IMainMonitoringService
    {
        MainMonitoringSession AddSession(Guid accountId, MainMonitoringSession session);
        IEnumerable<MainMonitoringSession> GetSessions(Guid accountId);
        bool RemoveSession(Guid accountId, Guid sessionId);
        Task<IEnumerable<Guid>> SendMessageToSessions(Guid accountId, List<Guid> sessionIds, byte[] bytes);
        Task SendMessageToAllSessions(Guid accountId, byte[] bytes);
    }
}