using System;
using System.Collections.Generic;

namespace CaesarServerLibrary.Events
{
    public class AddAccountsToTaskChatsEvent
    {
        public List<Guid> AccountIds { get; set; }
        public List<Guid> TaskIds { get; set; }
    }
}