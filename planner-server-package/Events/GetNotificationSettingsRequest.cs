using planner_server_package.Entities;
using System;
using System.Collections.Generic;

namespace planner_server_package.Events
{
    public class GetNotificationSettingsRequest
    {
        public List<Guid> AccountIds { get; set; }
    }
}
