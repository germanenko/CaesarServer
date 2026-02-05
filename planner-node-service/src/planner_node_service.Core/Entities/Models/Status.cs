using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class Status : ModelBase
    {
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public StatusDetails? StatusDetails { get; set; }
        public StatusType Type { get; set; }
        public int StatusCode { get; set; }
    }
}
