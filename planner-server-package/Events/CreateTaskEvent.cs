using planner_server_package.Entities;
using System;

namespace planner_server_package.Events
{
    public class CreateTaskEvent
    {
        public TaskBody Task { get; set; }
        public Guid CreatorId { get; set; }
    }
}