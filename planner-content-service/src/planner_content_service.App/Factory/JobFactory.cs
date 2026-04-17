using planner_client_package.Entities.Request;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IFactory;

namespace planner_content_service.App.Factory
{
    public class JobFactory : IJobFactory
    {
        public Job Create(JobBodyRequest body)
        {
            return body switch
            {
                MeetingBodyRequest m => new Meeting(m.Date, m.Members, false, m.Description),
                ReminderBodyRequest r => new Reminder(r.Date, false, r.Description),
                InformationBodyRequest i => new Information(false, i.Description),
                TaskBodyRequest t => new Core.Entities.Models.Task(t.PermormerIds, false, t.Description),
                _ => throw new NotSupportedException()
            };
        }
    }
}
