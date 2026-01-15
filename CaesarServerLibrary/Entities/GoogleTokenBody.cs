using System.ComponentModel.DataAnnotations;

namespace CaesarServerLibrary.Entities
{
    public class GoogleTokenBody
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

    }
}
