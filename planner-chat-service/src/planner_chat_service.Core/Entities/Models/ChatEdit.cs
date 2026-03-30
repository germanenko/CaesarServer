using planner_client_package.Entities;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_chat_service.Core.Entities.Models
{
    public class ChatEdit
    {
        public long Seq { get; set; }
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public Guid MessageId { get; set; }
        public ChatMessage Message { get; set; }
        public MessageAction Action { get; set; }
        public int Version { get; set; }

        public ChatEditBody ToBody()
        {
            return new ChatEditBody
            {
                Seq = Seq,
                ChatId = ChatId,
                MessageId = MessageId,
                Action = Action,
                Version = Version
            };
        }
    }
}
