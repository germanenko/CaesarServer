//using Planer_task_board.Core.Entities.Models;
using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using NpgsqlTypes;
using System.Text.Json;

namespace planner_node_service.Core.Entities.Models
{
    //[JsonDerivedType(typeof(Board), "board")]
    //[JsonDerivedType(typeof(Column), "column")]
    //[JsonDerivedType(typeof(TaskModel), "task")]
    //[JsonDerivedType(typeof(Chat), "chat")]
    //[JsonDerivedType(typeof(ChatMessage), "chatMessage")]
    public class Node : ModelBase
    {
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }
        public string BodyJson { get; set; }
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

        public NodeBody ToNodeBodyFromJson()
        {
            return JsonSerializer.Deserialize<NodeBody>(BodyJson);
        }
    }
}
