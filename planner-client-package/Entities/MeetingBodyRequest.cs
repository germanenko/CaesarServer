using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class MeetingBody : JobBody
    {
        public MeetingBody()
        {
            JobType = JobType.Meeting;
        }
        public string Theme { get; set; }
        public DateTime Date { get; set; }
        public List<Guid> Members { get; set; }
    }
}
