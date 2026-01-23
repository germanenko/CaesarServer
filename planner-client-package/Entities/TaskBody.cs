using planner_client_package.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace planner_client_package.Entities
{
    public class TaskBody : NodeBody
    {
        public string Description { get; set; }

        public int PriorityOrder { get; set; }

        public Status Status { get; set; }

        public TaskType TaskType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string HexColor { get; set; }

        public Guid MessageId { get; set; } = new();

        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}