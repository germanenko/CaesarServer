namespace planner_chat_service.Core.Entities.Request
{
    public class MessageDraftBody
    {
        public Guid ChatId { get; set; }
        public string Content { get; set; }
    }
}
