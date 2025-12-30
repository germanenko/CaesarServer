using CaesarServerLibrary.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CaesarServerLibrary.Entities
{
    public record BoardBody : NodeBody
    {
        [Required] public DateTime UpdatedAt { get; set; }
        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}