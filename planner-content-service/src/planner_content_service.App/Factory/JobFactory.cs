using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.App.Factory
{
    public class JobFactory : IJobFactory
    {
        public Job Create(JobBody body)
        {
            return body switch
            {
                MeetingBody m => new Meeting(m.Date, m.Members, JobType.Meeting, false, m.Description),
                ReminderBody r => new Reminder(r.Date, JobType.Reminder, false, r.Description),
                InformationBody i => new Information(JobType.Information, false, i.Description),
                TaskRequestBody t => new Core.Entities.Models.Task(t.PermormerIds, JobType.Task, false, t.Description),
                _ => throw new NotSupportedException()
            };
        }
    }
}
