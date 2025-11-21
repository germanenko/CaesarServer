using Planer_task_board.Core.Entities.Models;
using System.ComponentModel.DataAnnotations;

namespace Planer_task_board.Core.Entities.Request
{
    public class CreateColumnBody : ModelBase
    {
        [Required]
        public string Name { get; set; }
    }
}
