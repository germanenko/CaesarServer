using CaesarServerLibrary.Entities;
using System;

namespace CaesarServerLibrary.Events
{
    public class CreateTaskEvent
    {
        public TaskBody Task { get; set; }
        public Guid CreatorId { get; set; }
    }
}