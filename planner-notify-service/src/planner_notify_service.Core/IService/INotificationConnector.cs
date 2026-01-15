using System.Net.WebSockets;

namespace planner_notify_service.Core.IService
{
    public interface INotificationConnector
    {
        Task ConnectToNotificationService(Guid accountId, Guid sessionId, WebSocket socket);
    }
}