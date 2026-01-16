using System;
using System.Net.WebSockets;

namespace planner_client_package.Entities
{
    public class NotificationSession
    {
        public WebSocket Socket { get; set; }
        public Guid SessionId { get; set; }
    }
}