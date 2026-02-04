using planner_client_package.Entities;
using System.ComponentModel.DataAnnotations;

namespace planner_content_service.Core.Entities.Models
{
    public class TaskModel : Node
    {
        public string? Description { get; set; }

        [MaxLength(7)]
        public string? HexColor { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public TaskBody ToTaskBody()
        {
            return new TaskBody
            {
                Id = Id,
                Name = Name,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate,
                EndDate = EndDate,
                Props = Props,
                Type = Type
            };
        }

        public override NodeBody ToNodeBody()
        {
            return new TaskBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate,
                EndDate = EndDate,
            };
        }
    }
}