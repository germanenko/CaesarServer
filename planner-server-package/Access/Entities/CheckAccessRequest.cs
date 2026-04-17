using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Access.Entities
{
    public class CheckAccessRequest
    {
        public Guid AccountId { get; set; }
        public Guid NodeId { get; set; }
        public Permission MinRequiredPermission { get; set; }

        public CheckAccessRequest(Guid accountId, Guid nodeId, Permission minRequiredPermission)
        {
            AccountId = accountId;
            NodeId = nodeId;
            MinRequiredPermission = minRequiredPermission;
        }
    }
}
