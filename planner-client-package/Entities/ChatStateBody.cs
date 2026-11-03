using planner_client_package.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class ChatStateBody : IBody
    {
        public Guid ChatId { get; set; }
        public long? LastMessageSeq { get; set; }
        public long? EditCursorId { get; set; }
        public int UnreadCount { get; set; }
        public MessagePreviewBody LastPreview { get; set; }

        public ChatStateBody()
        {

        }
    }
}
