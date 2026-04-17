using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class InformationBodyRequest : JobBodyRequest
    {
        public InformationBodyRequest()
        {
            Type = JobType.Information;
        }
    }
}
