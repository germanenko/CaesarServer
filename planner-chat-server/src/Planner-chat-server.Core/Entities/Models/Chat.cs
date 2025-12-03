using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.Entities.Models
{
    public class Chat : Node
    {
        public string? Image { get; set; }
        public ChatType ChatType { get; set; }
        public Guid? TaskId { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
        public List<ChatSettings> ChatMemberships { get; set; } = new();
    }
}