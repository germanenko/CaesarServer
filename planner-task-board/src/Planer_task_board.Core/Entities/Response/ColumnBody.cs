using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.Entities.Response
{
    public class ColumnBody : ModelBase
    {
        public string Name { get; set; }
        public Guid BoardId { get; set; }
    }
}