using planner_common_package.Enums;
using System;

namespace planner_client_package.Entities
{
    public class AnalyticsActionBody
    {
        public int Id { get; set; }
        public Level Level { get; set; }
        public string AppVersion { get; set; }
        public string Platform { get; set; }
        public string Connection { get; set; }
        public string Window { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
