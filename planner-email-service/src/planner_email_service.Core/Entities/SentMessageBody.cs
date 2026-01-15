using System.ComponentModel.DataAnnotations;

namespace planner_email_service.Core.Entities
{
    public class SentMessageBody
    {
        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }
    }
}