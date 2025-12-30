using CaesarServerLibrary.Enums;
using System;
using System.Text.Json.Serialization;

namespace CaesarServerLibrary.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(BoardBody), "board")]
    [JsonDerivedType(typeof(ColumnBody), "column")]
    [JsonDerivedType(typeof(TaskBody), "task")]
    [JsonDerivedType(typeof(ChatBody), "chat")]
    [JsonDerivedType(typeof(MessageBody), "chatMessage")]
    public record NodeBody
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string Props { get; set; }
        public NodeLinkBody Link { get; set; }
    }
}
