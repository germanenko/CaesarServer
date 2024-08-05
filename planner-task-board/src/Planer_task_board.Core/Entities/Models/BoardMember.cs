namespace Planer_task_board.Core.Entities.Models
{
    public class BoardMember
    {
        public Board Board { get; set; }
        public Guid BoardId { get; set; }

        public Guid AccountId { get; set; }

        public string Role { get; set; }
    }
}