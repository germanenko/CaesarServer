using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class StatusHistory : ModelBase
    {
        public StatusBase OldStatus { get; set; }
        public Guid OldStatusId { get; set; }
        public StatusBase NewStatus { get; set; }
        public Guid NewStatusId { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public DateTime Date { get; set; }
        public Guid Actor { get; set; }
    }
}
