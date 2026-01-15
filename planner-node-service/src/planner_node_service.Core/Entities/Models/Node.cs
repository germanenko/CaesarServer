//using Planer_task_board.Core.Entities.Models;
using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using Newtonsoft.Json.Serialization;
using System.Text.Json;

namespace planner_node_service.Core.Entities.Models
{
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
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Deserialize<NodeBody>(BodyJson, options) ?? new NodeBody();
        }

        public static Node FromNodeBody(NodeBody nodeBody)
        {
            return new Node()
            {
                Id = nodeBody.Id,
                Type = nodeBody.Type,
                Name = nodeBody.Name,
                Props = nodeBody.Props,
                BodyJson = JsonSerializer.Serialize(nodeBody)
            };
        }
    }
}
