using CaesarServerLibrary.Entities;
using System;
using System.Collections.Generic;

namespace CaesarServerLibrary.Events
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