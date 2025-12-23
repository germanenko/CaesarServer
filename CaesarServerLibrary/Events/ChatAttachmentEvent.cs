using System;

namespace CaesarServerLibrary.Events
{
    public class ChatAttachmentEvent
    {
        public Guid ChatId { get; set; }
        public string FileName { get; set; }
        public Guid AccountId { get; set; }
    }
}