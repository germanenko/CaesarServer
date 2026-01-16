using planner_server_package.Enums;
using System.ComponentModel.DataAnnotations;

namespace planner_server_package.Entities
{
    public class TaskBody : NodeBody
    {
        public string Description { get; set; }

        public int PriorityOrder { get; set; }

        public Status Status { get; set; }

        public TaskType TaskType { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string HexColor { get; set; }

        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}