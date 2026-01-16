using System;
using System.Collections.Generic;

namespace planner_server_package.Events
{
    public class AddAccountsToTaskChatsEvent
    {
        public List<Guid> AccountIds { get; set; }
        public List<Guid> TaskIds { get; set; }
    }
}