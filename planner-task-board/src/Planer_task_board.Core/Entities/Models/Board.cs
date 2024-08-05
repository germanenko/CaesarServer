using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.Entities.Models
{
    public class Board
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<BoardMember> Members { get; set; } = new();
        public List<BoardColumn> Columns { get; set; } = new();

        public BoardBody ToBoardBody()
        {
            return new BoardBody
            {
                Id = Id,
                Name = Name
            };
        }
    }
}