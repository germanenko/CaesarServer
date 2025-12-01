using System.ComponentModel.DataAnnotations;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class TaskModel : Node
    {
        [MaxLength(128)]
        public string Name { get; set; }
        public string Description { get; set; }
        public int PriorityOrder { get; set; }

        [MaxLength(7)]
        public string? HexColor { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAtDate { get; set; } = DateTime.UtcNow;
        public Guid? ChatId { get; set; }
        public Guid CreatorId { get; set; }


        public TaskBody ToTaskBody()
        {
            return new TaskBody
            {
                Id = Id,
                Title = Name,
                Description = Description,
                HexColor = HexColor,
                PriorityOrder = PriorityOrder,
                StartDate = StartDate?.ToString("s"),
                EndDate = EndDate?.ToString("s"),
                ChatId = ChatId,
                UpdatedAt = UpdatedAt
            };
        }
    }
}