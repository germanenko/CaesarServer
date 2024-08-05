namespace Planer_task_board.Core.Entities.Models
{
    public class TaskPerformer
    {
        public Guid PerformerId { get; set; }
        public TaskModel Task { get; set; }
        public Guid TaskId { get; set; }
    }
}