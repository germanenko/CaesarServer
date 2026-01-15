using CaesarServerLibrary.Entities;

namespace Planer_task_board.Core.Entities.Models
{
    public class Column : Node
    {
        public ColumnBody ToColumnBody()
        {
            return new ColumnBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type
            };
        }
    }
}