using System;
using System.Collections.Generic;

namespace CaesarServerLibrary.Events
{
    public class InitChatEvent
    {
        public Guid AccountId { get; set; }
        public IEnumerable<Guid> SessionIds { get; set; } = new List<Guid>();
    }
}