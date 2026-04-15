using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class SourceMessage
    {
        public SourceMessage(Guid messageId, MessageState messageState)
        {
            MessageId = messageId;
            MessageState = messageState;
        }

        public Guid MessageId { get; set; }
        public MessageState MessageState { get; set; }
    }
}
