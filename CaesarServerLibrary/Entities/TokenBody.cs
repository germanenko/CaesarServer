using System.ComponentModel.DataAnnotations;

namespace CaesarServerLibrary.Entities
{
    public class TokenBody
    {
        [Required]
        public string Value { get; set; }
    }
}