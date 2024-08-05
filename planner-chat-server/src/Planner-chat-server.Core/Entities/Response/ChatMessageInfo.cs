using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.Entities.Response
{
    public class ChatMessageInfo
    {
        public Guid ChatId { get; set; }
        public ChatType ChatType { get; set; }
        public MessageBody Message { get; set; }
    }
}