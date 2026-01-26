using planner_common_package.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace planner_client_package.Entities
{
    public class BoardBody : NodeBody
    {
        [Required] public PublicationStatus PublicationStatus { get; set; }
    }
}