using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;

namespace Planner_chat_server.Core.Entities.Models
{
    public class Chat : Node
    {
        public string? Image { get; set; }
        public ChatType ChatType { get; set; }
        public Guid? TaskId { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
        public List<ChatSettings> ChatMemberships { get; set; } = new();

        public ChatBody ToChatBody()
        {
            return new ChatBody
            {
                Id = Id,
                ImageUrl = Image,
                ChatType = ChatType,
                Name = Name
            };
        }
    }
}