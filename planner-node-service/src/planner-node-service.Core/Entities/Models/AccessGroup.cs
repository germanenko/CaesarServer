using planner_node_service.Core.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public List<AccessGroupMember> Members { get; set; } = new();

        public AccessGroupBody ToAccessGroupBody()
        {
            return new AccessGroupBody
            {
                Id = Id,
                Name = Name
            };
        }
    }
}
