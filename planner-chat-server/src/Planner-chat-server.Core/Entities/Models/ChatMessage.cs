using Planner_chat_server.Core.Entities.Request;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.Entities.Models
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
                Type = messageType,
                Content = messageType == MessageType.File ? $"{Constants.WebUrlToChatAttachment}/{Content}" : Content,
                Date = SentAt,
                SenderId = SenderId,
                SenderDeviceId = deviceId,
                HasBeenRead = HasBeenRead
            };
        }
    }
}