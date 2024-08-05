namespace Planer_task_board.Core.Entities.Models
{
    public class BoardColumnTask
    {
        public BoardColumn Column { get; set; }
        public Guid ColumnId { get; set; }

        public TaskModel Task { get; set; }
        public Guid TaskId { get; set; }
    }
}