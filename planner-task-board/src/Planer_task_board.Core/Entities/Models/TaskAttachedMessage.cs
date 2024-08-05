namespace Planer_task_board.Core.Entities.Models
{
    public class TaskAttachedMessage
    {
        public TaskModel Task { get; set; }
        public Guid TaskId { get; set; }
        public Guid MessageId { get; set; }
    }
}