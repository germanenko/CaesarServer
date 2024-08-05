namespace Planner_chat_server.Core.Entities.Events
{
    public class CreateTaskChatEvent
    {
        public CreateTaskChat CreateTaskChat { get; set; }
        public bool IsSuccess { get; set; } = false;
    }

    public class CreateTaskChat
    {
        public Guid TaskId { get; set; }
        public Guid CreatorId { get; set; }
        public string ChatName { get; set; }
        public Guid? ChatId { get; set; }
    }
}