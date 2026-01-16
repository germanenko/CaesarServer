using planner_server_package.Enums;

namespace planner_content_service.Core.Entities.Models
{
    public class ChatMessage : Node
    {
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid SenderId { get; set; }
        public bool HasBeenRead { get; set; }
    }
}