using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class ReminderBodyRequest : JobBodyRequest
    {
        public ReminderBodyRequest()
        {
            Type = JobType.Reminder;
        }
        public DateTime Date { get; set; }
        public string Reminder { get; set; }
    }
}
