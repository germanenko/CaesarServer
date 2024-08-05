using System.ComponentModel.DataAnnotations;

namespace Planer_task_board.Core.Entities.Request
{
    public class UpdateDraftBody
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string? HexColor { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        public Guid? ModifiedTaskId { get; set; }
    }
}