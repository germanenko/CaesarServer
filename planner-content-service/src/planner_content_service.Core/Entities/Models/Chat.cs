using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_content_service.Core.Entities.Models
{
    public class Chat : Node
    {
        public string? Image { get; set; }
        public ChatType ChatType { get; set; }
        public Guid? TaskId { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
        public List<ChatSettings> ChatMemberships { get; set; } = new();

        public override NodeBody ToNodeBody()
        {
            return new ChatBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                ChatType = ChatType,
                ImageUrl = Image
            };
        }
    }
}