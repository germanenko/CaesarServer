using System.ComponentModel.DataAnnotations;
using planner_server_package.Enums;

namespace planner_chat_service.Core.Entities.Request
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