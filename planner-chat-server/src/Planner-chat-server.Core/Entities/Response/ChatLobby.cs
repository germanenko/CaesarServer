using System.Collections.Concurrent;

namespace Planner_chat_server.Core.Entities.Response
{
    public class ChatLobby
    {
        public ConcurrentDictionary<Guid, ChatSession> ActiveSessions { get; set; } = new();
        public List<Guid> AllChatUsers { get; set; } = new();
    }
}