using planner_client_package.Entities;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class Reminder : Job
    {
        public Reminder(
            DateTime remindAt,
            bool closeWhenChildrenCompleted,
            string? description)
            : base(closeWhenChildrenCompleted, description)
        {
            JobType = JobType.Reminder;
            RemindAt = remindAt;
        }

        public DateTime RemindAt { get; set; }

        public override NodeBody ToNodeBody()
        {
            return new ReminderBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate,
                EndDate = EndDate,
                Date = RemindAt,
                JobType = JobType,
                AttachedMessages = AttachedMessages.Select(x => x.ToBody()).ToList()
            };
        }
    }
}
