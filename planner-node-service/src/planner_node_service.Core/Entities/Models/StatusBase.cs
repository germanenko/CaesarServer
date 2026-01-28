using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class StatusBase : ModelBase
    {
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
    }
}
