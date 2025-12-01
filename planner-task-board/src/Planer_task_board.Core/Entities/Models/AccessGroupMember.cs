using Planer_task_board.Core.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Models
{
    public class AccessGroupMember : Node
    {
        public Guid AccessGroupId { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public Guid AccountId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public AccessGroupMemberBody ToAccessGroupMemberBody()
        {
            return new AccessGroupMemberBody
            {
                Id = Id,
                AccountId = AccountId,
                GroupId = AccessGroupId,
                JoinedAt = JoinedAt
            };
        }
    }
}
