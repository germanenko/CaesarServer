using Planner_chat_server.Core.Entities.Response;

namespace Planner_chat_server.Core.Entities.Events
{
    public class MessageSentToChatEvent
    {
        public IEnumerable<Guid> AccountIds { get; set; } = new List<Guid>();
        public IEnumerable<AccountSessions> AccountSessions { get; set; } = new List<AccountSessions>();
        public byte[] Message { get; set; }
    }
}