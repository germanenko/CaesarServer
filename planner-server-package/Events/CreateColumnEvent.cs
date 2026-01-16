using planner_server_package.Entities;
using System;

namespace planner_server_package.Events
{
    public class CreateColumnEvent
    {
        public ColumnBody Column { get; set; }
        public Guid CreatorId { get; set; }
    }
}