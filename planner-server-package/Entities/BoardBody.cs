using planner_common_package.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace planner_server_package.Entities
{
    public class BoardBody : NodeBody
    {
        public PublicationStatus PublicationStatus { get; set; }
    }
}