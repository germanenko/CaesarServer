using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Response
{
    public class AccessGroupMemberBody
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid GroupId { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
