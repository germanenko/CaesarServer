using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.Entities.Models
{
    public class Board : ModelBase
    { 
        public string Name { get; set; }

        public BoardBody ToBoardBody()
        {
            return new BoardBody
            {
                Id = Id,
                Name = Name,
                UpdatedAt = UpdatedAt
            };
        }
    }
}