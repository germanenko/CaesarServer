using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class ChatUserStateBody
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid AccountId { get; set; }
        public long LastReadSeq { get; set; }
        public long CachedUnreadCount { get; set; }
    }
}
