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
        public Information(JobType jobType, bool closeWhenChildrenCompleted, string? description) : base(jobType, closeWhenChildrenCompleted, description)
        {
        }
    }
}
