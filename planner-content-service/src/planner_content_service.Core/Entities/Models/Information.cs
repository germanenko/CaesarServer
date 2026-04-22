using planner_client_package.Entities;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class Information : Job
    {
        public Information(bool closeWhenChildrenCompleted, string? description) : base(closeWhenChildrenCompleted, description)
        {
            JobType = JobType.Information;
        }

        public override NodeBody ToNodeBody()
        {
            return new InformationBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate,
                EndDate = EndDate,
                JobType = JobType
            };
        }
    }
}
