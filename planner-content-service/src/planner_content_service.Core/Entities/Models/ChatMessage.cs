using planner_client_package.Entities;
using planner_common_package.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace planner_content_service.Core.Entities.Models
{
    public class ChatMessage : Node
    {
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid SenderId { get; set; }
        public bool HasBeenRead { get; set; }

        public override NodeBody ToNodeBody()
        {
            return new MessageBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                Date = SentAt,
                SenderId = SenderId,
            };
        }
    }
}