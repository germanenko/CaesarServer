using System.Net.WebSockets;

namespace planner_notify_service.Core.IService
{
    public interface IMainMonitoringConnector
    {
        Task ConnectToMainMonitoringService(Guid accountId, Guid sessionId, WebSocket socket);
    }
}