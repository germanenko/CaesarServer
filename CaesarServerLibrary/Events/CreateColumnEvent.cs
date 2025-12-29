using CaesarServerLibrary.Entities;
using System;

namespace CaesarServerLibrary.Events
{
    public class CreateColumnEvent
    {
        public ColumnBody Column { get; set; }
        public Guid CreatorId { get; set; }
    }
}