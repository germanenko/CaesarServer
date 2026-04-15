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
        public SourceMessage(Guid id, MessageState state)
        {
            MessageId = id;
            MessageState = state;
        }

        public Guid MessageId { get; set; }
        public MessageState MessageState { get; set; }
    }
}
