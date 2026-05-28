using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace planner_content_service.Core.Entities.Models
{
    public class Meeting : Job
    {
        public Meeting(
            DateTime meetAt,
            List<Guid> memberIds,
            bool closeWhenChildrenCompleted,
            string? description)
            : base(closeWhenChildrenCompleted, description)
        {
            JobType = JobType.Meeting;
            MeetAt = meetAt;
            MemberIds = memberIds;
        }

        public DateTime MeetAt { get; set; }
        public List<Guid> MemberIds { get; set; }

        public override NodeBody ToNodeBody()
        {
            var body = FillBody(new MeetingBody());

            body.Members = MemberIds;
            body.Date = MeetAt;

            return body;
        }
    }
}
