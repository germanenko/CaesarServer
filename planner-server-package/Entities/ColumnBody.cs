using planner_server_package.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace planner_server_package.Entities
{
    public class ColumnBody : NodeBody
    {
        [Required] public DateTime UpdatedAt { get; set; }
        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}
