namespace Planer_task_board.Core.Entities.Events
{
    public class AddAccountsToTaskChatsEvent
    {
        public List<Guid> AccountIds { get; set; } = new();
        public List<Guid> TaskIds { get; set; } = new();
    }
}