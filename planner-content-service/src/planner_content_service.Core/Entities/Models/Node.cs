using planner_client_package.Entities;
using planner_common_package;
using planner_common_package.Enums;
using System.Text.Json.Serialization;

namespace planner_content_service.Core.Entities.Models
{
    [JsonDerivedType(typeof(Board), Discriminator.Board)]
    [JsonDerivedType(typeof(Column), Discriminator.Column)]
    [JsonDerivedType(typeof(TaskModel), Discriminator.Task)]
    [JsonDerivedType(typeof(Chat), Discriminator.Chat)]
    [JsonDerivedType(typeof(ChatMessage), Discriminator.ChatMessage)]
    public abstract class Node : ModelBase
    {
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }

        public abstract NodeBody ToNodeBody();
    }
}
