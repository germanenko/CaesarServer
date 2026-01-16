using planner_server_package.Entities;
using System;
using System.Collections.Generic;

namespace planner_server_package.Events
{
    public class MessageSentToChatEvent
    {
        public IEnumerable<Guid> AccountIds { get; set; } = new List<Guid>();
        public IEnumerable<AccountSessions> AccountSessions { get; set; } = new List<AccountSessions>();
        public byte[] Message { get; set; }
    }
}