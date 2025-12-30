using CaesarServerLibrary.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CaesarServerLibrary.Entities
{
    public record ColumnBody : NodeBody
    {
        [Required] public DateTime UpdatedAt { get; set; }
        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}
