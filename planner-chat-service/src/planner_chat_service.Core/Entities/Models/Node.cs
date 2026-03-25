using planner_client_package.Entities;
using planner_common_package;
using planner_common_package.Enums;
using System.Data.Common;
using System.Text.Json.Serialization;

namespace planner_chat_service.Core.Entities.Models
{
    [JsonDerivedType(typeof(Chat), Discriminator.Chat)]
    [JsonDerivedType(typeof(ChatMessage), Discriminator.ChatMessage)]
    public abstract class Node
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }

        public abstract NodeBody ToNodeBody();
    }
}
