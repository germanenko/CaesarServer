namespace Planer_task_board.Core.Entities.Models
{
    public class BoardColumnMember
    {
        public BoardColumn Column { get; set; }
        public Guid ColumnId { get; set; }
        public Guid AccountId { get; set; }

        public string Role { get; set; }
    }
}