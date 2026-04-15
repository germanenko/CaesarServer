using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class Task : Job
    {
        public Task(
            List<Guid> performerIds,
            bool closeWhenChildrenCompleted,
            string? description)
            : base(closeWhenChildrenCompleted, description)
        {
            PermormerIds = performerIds;
        }

        public List<Guid> PermormerIds { get; set; }
    }
}
