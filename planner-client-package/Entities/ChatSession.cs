using System;
using System.Net.WebSockets;

namespace planner_client_package.Entities
{
    public class ChatSession
    {
        public Guid AccountId { get; set; }
        public WebSocket Ws { get; set; }
        public Guid SessionId { get; set; }
    }
}