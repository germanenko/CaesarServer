using System.ComponentModel.DataAnnotations;

namespace Planer_task_board.Core.Entities.Request
{
    public class CreateBoardBody
    {
        [Required]
        public string Name { get; set; }
    }
}