//using Planer_task_board.Core.Entities.Models;
using Newtonsoft.Json.Serialization;
using planner_client_package.Entities;
using planner_common_package.Enums;
using System.Text.Json;

namespace planner_node_service.Core.Entities.Models
{
    public class Node : TrackableEntity
    {
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }
        public NodeBody ToNodeBody()
        {
            return new NodeBody()
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                Version = Version
            };
        }
    }
}
