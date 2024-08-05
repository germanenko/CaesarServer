using System.Net.WebSockets;

namespace Planner_chat_server.Core.Entities.Response
{
    public class ChatSession
    {
        public Guid AccountId { get; set; }
        public WebSocket Ws { get; set; }
        public Guid SessionId { get; set; }
    }
}