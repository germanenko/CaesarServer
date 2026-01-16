using System;

namespace planner_server_package.Events
{
    public class ChatImageEvent
    {
        public Guid ChatId { get; set; }
        public string Filename { get; set; }
    }
}