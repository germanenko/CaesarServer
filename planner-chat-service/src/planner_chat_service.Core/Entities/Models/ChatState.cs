using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_chat_service.Core.Entities.Models
{
    public class ChatState
    {
        public Guid ChatId { get; set; }
        public long LastMessageSeq { get; set; }
        public ChatMessage LastMessage { get; set; }
        public long EditCursorId { get; set; }
        public ChatEdit EditCursor { get; set; }
        public int UnreadCount { get; set; }
        public DateTime LastPreview { get; set; }
    }
}
