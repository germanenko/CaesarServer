using CaesarServerLibrary.Enums;
using System;

namespace CaesarServerLibrary.Entities
{
    public record MessageBody : NodeBody
    {
        public Guid Id { get; set; }
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public Guid SenderId { get; set; }
        public Guid ChatId { get; set; }
        public Guid? SenderDeviceId { get; set; }
        public bool HasBeenRead { get; set; }
    }
}