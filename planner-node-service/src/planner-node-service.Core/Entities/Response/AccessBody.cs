using Planer_task_board.Core.Entities.Response;
using planner_node_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Response
{
    public class AccessBody
    {
        public List<AccessRight> AccessRights { get; set; }
        public List<AccessGroupBody> AccessGroups { get; set; }
        public List<AccessGroupMemberBody> AccessGroupMembers { get; set; }
        public List<ProfileBody> Profiles { get; set; }
    }
}
