using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class StatusHistory : ModelBase
    {
        public Status OldStatus { get; set; }
        public Guid OldStatusId { get; set; }
        public Status NewStatus { get; set; }
        public Guid NewStatusId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UpdatedById { get; set; }
    }
}
