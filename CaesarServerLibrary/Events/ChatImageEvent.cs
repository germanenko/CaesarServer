using System;

namespace CaesarServerLibrary.Events
{
    public class ChatImageEvent
    {
        public Guid ChatId { get; set; }
        public string Filename { get; set; }
    }
}