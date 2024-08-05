using System.ComponentModel.DataAnnotations;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Request
{
    public class UpdateTaskBody
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        public int PriorityOrder { get; set; }

        [EnumDataType(typeof(TaskState))]
        public TaskState Status { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string? HexColor { get; set; }
    }
}