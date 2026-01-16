using planner_server_package.Entities;
using System;
using System.Collections.Generic;

namespace planner_server_package.Events
{
    public class CreatePersonalChatEvent
    {
        public ChatBody Chat { get; set; }
        public IEnumerable<ChatMembership> Participants { get; set; } = new List<ChatMembership>();
    }

    public class ChatMembership
    {
        public Guid AccountId { get; set; }
        public Guid ChatMembershipId { get; set; }
    }
}