using System.ComponentModel.DataAnnotations;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class TaskModel : Node
    {
        public string Description { get; set; }

        [MaxLength(7)]
        public string? HexColor { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public TaskBody ToTaskBody()
        {
            return new TaskBody
            {
                Id = Id,
                Title = Name,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate?.ToString("s"),
                EndDate = EndDate?.ToString("s"),
                UpdatedAt = UpdatedAt
            };
        }
    }
}