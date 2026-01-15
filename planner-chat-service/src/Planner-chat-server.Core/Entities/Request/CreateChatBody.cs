using System.ComponentModel.DataAnnotations;

namespace Planner_chat_server.Core.Entities.Request
{
    public class CreateChatBody
    {
        public Guid Id { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [Required]
        public string Name { get; set; }
    }
}