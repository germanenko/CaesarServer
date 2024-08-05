namespace Planner_chat_server.Core.Entities.Events
{
    public class ChatImageEvent
    {
        public Guid ChatId { get; set; }
        public string Filename { get; set; }
    }
}