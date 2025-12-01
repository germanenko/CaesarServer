using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Response
{
    public class TaskBody : ModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int PriorityOrder { get; set; }

        public string? HexColor { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public Guid? ChatId { get; set; }
    }
}