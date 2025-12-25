using System.ComponentModel.DataAnnotations;
using CaesarServerLibrary.Enums;

namespace Planner_chat_server.Core.Entities.Request
{
    public class CreateMessageBody
    {
        public Guid Id { get; set; }

        public MessageType Type { get; set; }

        [Required]
        public string Content { get; set; }
    }

    public class SentMessage
    {
        public CreateMessageBody? MessageBody { get; set; }
        public Guid? LastMessageReadId { get; set; }
        public Guid? DeviceId { get; set; }
    }
}