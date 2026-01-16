using System.ComponentModel.DataAnnotations;

namespace planner_client_package.Entities
{
    public class TokenBody
    {
        [Required]
        public string Value { get; set; }
    }
}