using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_chat_service.Core.Entities.Models
{
    public class ChatUserState
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public Guid AccountId { get; set; }
        public long LastReadSeq { get; set; }
        public long CachedUnreadCount { get; set; }

        public ChatUserStateBody ToBody()
        {
            return new ChatUserStateBody
            {
                Id = Id,
                ChatId = ChatId,
                AccountId = AccountId,
                CachedUnreadCount = CachedUnreadCount,
                LastReadSeq = LastReadSeq
            };
        }
    }
}
