using CaesarServerLibrary.Entities;

namespace Planer_task_board.Core.Entities.Models
{
    public class Board : Node
    {
        public string HexColor { get; set; } = "FFFFFF";
        public BoardBody ToBoardBody()
        {
            return new BoardBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type
            };
        }
    }
}