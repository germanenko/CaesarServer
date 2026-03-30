using planner_chat_service.Core.Entities.Request;
using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_chat_service.Core.Entities.Models
{
    public class ChatMessage : Node
    {
        public long Seq { get; set; }
        public MessageType MessageType { get; set; }
        public Chat? Chat { get; set; }
        public Guid? ChatId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }

        public Guid SenderId { get; set; }
        public Guid? SenderDeviceId { get; set; }
        public bool HasBeenRead { get; set; }
        public bool IsDeleted { get; set; }

        public override MessageBody ToNodeBody()
        {
            var messageType = MessageType;

            return new MessageBody
            {
                Id = Id,
                Name = Name,
                Type = NodeType.Message,
                MessageType = MessageType,
                Content = messageType == MessageType.File ? $"{Constants.WebUrlToChatAttachment}/{Content}" : Content,
                SentAt = SentAt,
                SenderId = SenderId,
                SenderDeviceId = SenderDeviceId,
                HasBeenRead = HasBeenRead,
                Link = new NodeLinkBody() { ChildId = Id, ParentId = ChatId.Value }
            };
        }
    }
}