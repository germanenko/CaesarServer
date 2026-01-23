using planner_client_package.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace planner_client_package.Entities
{
    public class ColumnBody : NodeBody
    {
        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}
