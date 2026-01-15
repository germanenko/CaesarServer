using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Core.Entities.Request
{
    public class MessageDraftBody
    {
        public Guid ChatId { get; set; }
        public string Content { get; set; }
    }
}
