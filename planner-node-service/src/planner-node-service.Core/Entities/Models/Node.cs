using Planer_task_board.Core.Entities.Models;
using planner_node_service.Core.Entities.Response;
using planner_node_service.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    [JsonDerivedType(typeof(Board), "board")]
    [JsonDerivedType(typeof(Column), "column")]
    [JsonDerivedType(typeof(TaskModel), "task")]
    [JsonDerivedType(typeof(Chat), "chat")]
    [JsonDerivedType(typeof(ChatMessage), "chatMessage")]
    public class Node : ModelBase
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
                Type = Type
            };
        }
    }
}
