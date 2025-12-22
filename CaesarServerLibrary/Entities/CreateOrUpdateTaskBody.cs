using System;
using System.ComponentModel.DataAnnotations;
using CaesarServerLibrary.Enums;

namespace CaesarServerLibrary.Entities
{
    public class CreateOrUpdateTaskBody
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        public int PriorityOrder { get; set; }

        public Status Status { get; set; }

        public TaskType Type { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string HexColor { get; set; }

        public Guid MessageId { get; set; } = new();

        public Guid? ColumnId { get; set; }

        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}