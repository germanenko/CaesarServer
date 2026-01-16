using System.ComponentModel.DataAnnotations;

namespace planner_client_package.Entities
{
    public class GoogleTokenBody
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

    }
}
