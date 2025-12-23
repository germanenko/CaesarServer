using System;
using System.Net.WebSockets;

namespace CaesarServerLibrary.Entities
{
    public class ChatSession
    {
        public Guid AccountId { get; set; }
        public WebSocket Ws { get; set; }
        public Guid SessionId { get; set; }
    }
}