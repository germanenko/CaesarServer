using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class CreateOrUpdateReminderBody : CreateOrUpdateJobBody
    {
        public DateTime Date;
        public string Reminder;
    }
}
