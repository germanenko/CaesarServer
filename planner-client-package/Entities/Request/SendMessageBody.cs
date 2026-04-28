using planner_client_package.Interface;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class SendMessageBody : IRequest
    {
        public Guid Id { get; set; }
        public NodeLinkBody Link { get; set; }
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public Guid SenderId { get; set; }
        public Guid? SenderDeviceId { get; set; }
    }
}
