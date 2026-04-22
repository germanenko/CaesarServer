using planner_client_package.Entities;
using planner_common_package.Enums;
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
            JobType = JobType.Task;
            PerformerIds = performerIds;
        }

        public List<Guid> PerformerIds { get; set; }

        public override NodeBody ToNodeBody()
        {
            return new TaskBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate,
                EndDate = EndDate,
                PermormerIds = PerformerIds,
                JobType = JobType
            };
        }
    }
}
