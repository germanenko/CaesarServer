using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.Entities.Response
{
    public class MessageBody : ModelBase
    {
        public MessageType Type { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public Guid SenderId { get; set; }
        public Guid ChatId { get; set; }
    }
}