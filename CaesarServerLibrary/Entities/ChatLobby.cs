using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CaesarServerLibrary.Entities
{
    public class ChatLobby
    {
        public ConcurrentDictionary<Guid, ChatSession> ActiveSessions { get; set; } = new();
        public List<Guid> AllChatUsers { get; set; } = new();
    }
}