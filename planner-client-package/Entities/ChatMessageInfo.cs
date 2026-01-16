using planner_client_package.Enums;
using System;

namespace planner_client_package.Entities
{
    public class ChatMessageInfo
    {
        public Guid ChatId { get; set; }
        public ChatType ChatType { get; set; }
        public MessageBody Message { get; set; }
    }
}