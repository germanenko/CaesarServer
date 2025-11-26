using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Planer_task_board.Core.Entities.Request
{
    public class CreateColumnBody
    {
        [Required] public Guid Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public DateTime UpdatedAt { get; set; }
        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}
