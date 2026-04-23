using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_chat_service.Core.Entities.Models
{
    public class Chat : Node
    {
        public string? Image { get; set; }
        public ChatType ChatType { get; set; }
        public Guid? TaskId { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
        public ICollection<ChatSettings> ChatSettings { get; set; } = new List<ChatSettings>();

        public override ChatBody ToNodeBody()
        {
            return new ChatBody
            {
                Id = Id,
                Type = NodeType.Chat,
                ImageUrl = Image,
                ChatType = ChatType,
                Name = Name,
                SyncKind = SyncKind.Scope
            };
        }
    }
}