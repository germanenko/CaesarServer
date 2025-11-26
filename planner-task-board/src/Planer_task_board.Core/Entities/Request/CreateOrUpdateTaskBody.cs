using System.ComponentModel.DataAnnotations;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Request
{
    public class CreateOrUpdateTaskBody : ModelBase
    {
        [Required]
        [MaxLength(128)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Range(0, 10)]
        public int PriorityOrder { get; set; }

        [EnumDataType(typeof(Status))]
        public Status Status { get; set; }

        [EnumDataType(typeof(TaskType))]
        public TaskType Type { get; set; }

        [DataType(DataType.DateTime)]
        public string? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public string? EndDate { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string? HexColor { get; set; }

        public Guid MessageId { get; set; } = new();

        public Guid? ColumnId { get; set; }

        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}