using planner_server_package.Entities;
using System;
using System.Collections.Generic;

namespace planner_server_package.Events
{
    public class CreatePersonalChatEvent
    {
        public ChatBody Chat { get; set; }
        public IEnumerable<Guid> ParticipantIds { get; set; } = new List<Guid>();
    }
}