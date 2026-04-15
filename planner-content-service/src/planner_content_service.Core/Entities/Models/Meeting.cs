using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class Meeting : Job
    {
        public Meeting(
            DateTime meetAt,
            List<Guid> memberIds,
            JobType jobType,
            bool closeWhenChildrenCompleted,
            string? description)
            : base(jobType, closeWhenChildrenCompleted, description)
        {
            MeetAt = meetAt;
            MemberIds = memberIds;
        }

        public DateTime MeetAt { get; set; }
        public List<Guid> MemberIds { get; set; }
    }
}
