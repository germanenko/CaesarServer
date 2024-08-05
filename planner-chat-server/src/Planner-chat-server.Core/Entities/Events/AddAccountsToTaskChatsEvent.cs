namespace Planner_chat_server.Core.Entities.Events
{
    public class AddAccountsToTaskChatsEvent
    {
        public List<Guid> AccountIds { get; set; }
        public List<Guid> TaskIds { get; set; }
    }
}