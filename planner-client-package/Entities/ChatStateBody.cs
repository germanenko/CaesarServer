using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class ChatStateBody
    {
        public Guid ChatId { get; set; }
        public long? LastMessageSeq { get; set; }
        public long? EditCursorId { get; set; }
        public int UnreadCount { get; set; }
        public MessagePreviewBody LastPreview { get; set; }
    }
}
