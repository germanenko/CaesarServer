using System.ComponentModel.DataAnnotations;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class TaskModel
    {
        public Guid Id { get; set; }

        [MaxLength(128)]
        public string Title { get; set; }
        public string Description { get; set; }
        public int PriorityOrder { get; set; }
        public string Status { get; set; }

        [MaxLength(7)]
        public string? HexColor { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAtDate { get; set; } = DateTime.UtcNow;
        public bool IsDraft { get; set; }
        public string Type { get; set; }

        public TaskModel? DraftOfTask { get; set; }
        public Guid? DraftOfTaskId { get; set; }
        public Guid? ChatId { get; set; }

        public List<BoardColumnTask> Columns { get; set; } = new();

        public DeletedTask? DeletedTask { get; set; }

        public Guid CreatorId { get; set; }
        public List<TaskAttachedMessage> AttachedMessages { get; set; } = new();


        public TaskBody ToTaskBody()
        {
            return new TaskBody
            {
                Id = Id,
                Title = Title,
                Description = Description,
                HexColor = HexColor,
                PriorityOrder = PriorityOrder,
                Status = Enum.Parse<TaskState>(Status),
                StartDate = StartDate?.ToString("s"),
                EndDate = EndDate?.ToString("s"),
                ChatId = ChatId
            };
        }
    }
}