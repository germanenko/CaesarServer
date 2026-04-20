using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class InformationBody : JobBody
    {
        public InformationBody()
        {
            JobType = JobType.Information;
        }
    }
}
