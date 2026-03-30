using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class ChatEditBody
    {
        public long Seq { get; set; }
        public Guid ChatId { get; set; }
        public Guid MessageId { get; set; }
        public MessageAction Action { get; set; }
        public int Version { get; set; }
    }
}
