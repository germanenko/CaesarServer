using System;

namespace planner_server_package.Events
{
    public class ChatAttachmentEvent
    {
        public Guid ChatId { get; set; }
        public string FileName { get; set; }
        public Guid AccountId { get; set; }
    }
}