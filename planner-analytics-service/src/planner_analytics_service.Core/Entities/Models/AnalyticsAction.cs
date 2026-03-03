using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_analytics_service.Core.Entities.Models
{
    public class AnalyticsAction
    {
        public Guid Id { get; set; }
        public Level Level { get; set; }
        public string AppVersion { get; set; }
        public string Platform { get; set; }
        public string Connection { get; set; }
        public string Window { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        public AnalyticsActionBody ToBody()
        {
            return new AnalyticsActionBody
            {
                Id = Id,
                Level = Level,
                AppVersion = AppVersion,
                Platform = Platform,
                Connection = Connection,
                Window = Window,
                Description = Description,
                Date = Date
            };
        }
    }
}
