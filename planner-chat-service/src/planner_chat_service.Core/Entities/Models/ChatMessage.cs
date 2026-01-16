using planner_server_package.Entities;
using planner_server_package.Enums;
using planner_chat_service.Core.Entities.Request;

namespace planner_chat_service.Core.Entities.Models
{
    public class ChatMessage : Node
    {
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid SenderId { get; set; }
        public bool HasBeenRead { get; set; }

        public CreateMessageBody ToCreateMessageBody()
        {
            var messageType = MessageType;

            return new CreateMessageBody
            {
                Type = messageType,
                Content = messageType == MessageType.File ? $"{Constants.WebUrlToChatAttachment}/{Content}" : Content,
            };
        }

        public MessageBody ToMessageBody(Guid? deviceId = null)
        {
            var messageType = MessageType;

            return new MessageBody
            {
                Id = Id,
                Type = NodeType.Message,
                MessageType = MessageType,
                Content = messageType == MessageType.File ? $"{Constants.WebUrlToChatAttachment}/{Content}" : Content,
                Date = SentAt,
                SenderId = SenderId,
                SenderDeviceId = deviceId,
                HasBeenRead = HasBeenRead
            };
        }
    }
}