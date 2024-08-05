using System.ComponentModel.DataAnnotations;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Request
{
    public class CreateDraftBody
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string? HexColor { get; set; }

        [EnumDataType(typeof(TaskType))]
        public TaskType Type { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        public Guid? ModifiedTaskId { get; set; }
        public List<Guid> MessageIds { get; set; } = new();
    }
}