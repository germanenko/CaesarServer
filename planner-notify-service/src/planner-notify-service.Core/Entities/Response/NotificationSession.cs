using System.Net.WebSockets;

namespace planner_notify_service.Core.Entities.Response
{
    public class NotificationSession
    {
        public WebSocket Socket { get; set; }
        public Guid SessionId { get; set; }
    }
}