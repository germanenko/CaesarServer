using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Events
{
    public class MessageEditedEvent
    {
        public MessageEditedEvent(Guid messageId, MessageState state)
        {
            MessageId = messageId;
            State = state;
        }

        public Guid MessageId { get; set; }
        public MessageState State { get; set; }
    }
}
