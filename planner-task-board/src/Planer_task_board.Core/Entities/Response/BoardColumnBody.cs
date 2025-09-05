using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.Entities.Response
{
    public class BoardColumnBody : ModelBase
    {
        public string Name { get; set; }
        public Guid BoardId { get; set; }
    }
}