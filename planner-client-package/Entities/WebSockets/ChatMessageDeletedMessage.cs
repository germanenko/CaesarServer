using planner_common_package.Entities.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.WebSockets
{
    public record ChatMessageDeletedMessage : IMessagePayload
    {
        public Guid MessageId { get; set; }

        public ChatMessageDeletedMessage(Guid messageId)
        {
            MessageId = messageId;
        }
    }
}
