using System.ComponentModel.DataAnnotations;

namespace Planner_Auth.Core.Entities.Request
{
    public class TokenBody
    {
        [Required]
        public string Value { get; set; }
    }
}