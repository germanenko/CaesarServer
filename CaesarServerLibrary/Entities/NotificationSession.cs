using System;
using System.Net.WebSockets;

namespace CaesarServerLibrary.Entities
{
    public class NotificationSession
    {
        public WebSocket Socket { get; set; }
        public Guid SessionId { get; set; }
    }
}