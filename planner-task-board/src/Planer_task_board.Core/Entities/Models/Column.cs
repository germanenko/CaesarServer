using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.Entities.Models
{
    public class Column : Node
    {
        public string Name { get; set; }

        public BoardColumnBody ToBoardColumnBody()
        {
            return new BoardColumnBody
            {
                Id = Id,
                Name = Name,
                UpdatedAt = UpdatedAt
            };
        }
    }
}