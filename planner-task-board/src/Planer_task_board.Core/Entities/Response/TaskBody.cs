using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Response
{
    public class TaskBody : NodeBody
    {
        public string Description { get; set; }

        public string? HexColor { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}