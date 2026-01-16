using planner_client_package.Enums;
using System;
using System.Text.Json.Serialization;

namespace planner_client_package.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(BoardBody), "board")]
    [JsonDerivedType(typeof(ColumnBody), "column")]
    [JsonDerivedType(typeof(TaskBody), "task")]
    [JsonDerivedType(typeof(ChatBody), "chat")]
    [JsonDerivedType(typeof(MessageBody), "chatMessage")]
    public class NodeBody
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string Props { get; set; }
        public NodeLinkBody Link { get; set; }
    }
}
