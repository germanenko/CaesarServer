using System;
using System.Collections.Generic;

namespace planner_client_package.Entities
{
    public class AccountSessions
    {
        public Guid AccountId { get; set; }
        public List<Guid> SessionIds { get; set; } = new();
    }
}