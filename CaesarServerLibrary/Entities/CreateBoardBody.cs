using CaesarServerLibrary.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace CaesarServerLibrary.Entities
{
    public class CreateBoardBody
    {
        [Required] public Guid Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public DateTime UpdatedAt { get; set; }
        [Required] public PublicationStatus PublicationStatus { get; set; }
        public string Props { get; set; }
    }
}