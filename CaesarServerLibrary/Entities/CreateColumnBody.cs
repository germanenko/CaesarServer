using CaesarServerLibrary.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CaesarServerLibrary.Entities
{
    public class CreateColumnBody
    {
        [Required] public Guid Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public DateTime UpdatedAt { get; set; }
        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}
