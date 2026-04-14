using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class Meeting : Job
    {
        public DateTime MeetAt { get; set; }
        public List<Guid> MemberIds { get; set; }
    }
}
