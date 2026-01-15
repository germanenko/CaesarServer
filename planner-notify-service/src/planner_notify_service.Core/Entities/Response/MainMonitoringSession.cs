using System.Net.WebSockets;

namespace planner_notify_service.Core.Entities.Response
{
    public class MainMonitoringSession
    {
        public Guid SessionId { get; set; }
        public WebSocket Socket { get; set; }
    }
}