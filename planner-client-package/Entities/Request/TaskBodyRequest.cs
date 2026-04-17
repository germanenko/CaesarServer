using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class TaskBodyRequest : JobBodyRequest
    {
        public TaskBodyRequest()
        {
            Type = JobType.Task;
        }

        public List<Guid> PermormerIds { get; set; }
    }
}
