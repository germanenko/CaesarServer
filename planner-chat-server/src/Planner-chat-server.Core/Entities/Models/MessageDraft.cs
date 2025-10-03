using Planner_chat_server.Core.Entities.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Core.Entities.Models
{
    public class MessageDraft
    {
        public int Id { get; set; }
        public Guid MembershipId { get; set; }
        public ChatMembership ChatMembership { get; set; }
        public string Content { get; set; }

        public MessageDraftBody ToMessageDraftBody()
        {
            return new MessageDraftBody()
            {
                Content = Content,
                ChatId = ChatMembership.ChatId
            };
        }
    }
}
