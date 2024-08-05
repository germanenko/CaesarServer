using Planner_chat_server.Core.Entities.Request;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.Entities.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid SenderId { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }

        public CreateMessageBody ToCreateMessageBody()
        {
            var messageType = Enum.Parse<MessageType>(Type);

            return new CreateMessageBody
            {
                Type = messageType,
                Content = messageType == MessageType.File ? $"{Constants.WebUrlToChatAttachment}/{Content}" : Content,
            };
        }

        public MessageBody ToMessageBody()
        {
            var messageType = Enum.Parse<MessageType>(Type);

            return new MessageBody
            {
                Id = Id,
                Type = messageType,
                Content = messageType == MessageType.File ? $"{Constants.WebUrlToChatAttachment}/{Content}" : Content,
                Date = SentAt,
                SenderId = SenderId,
            };
        }
    }
}