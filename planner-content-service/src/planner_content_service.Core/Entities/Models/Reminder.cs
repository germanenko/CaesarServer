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
            RemindAt = remindAt;
        }

        public DateTime RemindAt { get; set; }
    }
}
