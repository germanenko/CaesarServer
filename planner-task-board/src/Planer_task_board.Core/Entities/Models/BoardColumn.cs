using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.Entities.Models
{
    public class BoardColumn
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Board Board { get; set; }
        public Guid BoardId { get; set; }
        public List<BoardColumnMember> Members { get; set; } = new();
        public List<BoardColumnTask> Tasks { get; set; } = new();

        public BoardColumnBody ToBoardColumnBody()
        {
            return new BoardColumnBody
            {
                Id = Id,
                Name = Name,
            };
        }
    }
}