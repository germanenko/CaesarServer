using CaesarServerLibrary.Enums;
using System;

namespace CaesarServerLibrary.Entities
{
    public class ChatMessageInfo
    {
        public Guid ChatId { get; set; }
        public ChatType ChatType { get; set; }
        public MessageBody Message { get; set; }
    }
}