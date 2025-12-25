using CaesarServerLibrary.Enums;
using System;
using System.Text.Json.Serialization;

namespace CaesarServerLibrary.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    [JsonDerivedType(typeof(BoardBody), typeDiscriminator: 0)]
    [JsonDerivedType(typeof(ColumnBody), typeDiscriminator: 1)]
    [JsonDerivedType(typeof(TaskBody), typeDiscriminator: 2)]
    [JsonDerivedType(typeof(ChatBody), typeDiscriminator: 3)]
    [JsonDerivedType(typeof(MessageBody), typeDiscriminator: 4)]
    public class NodeBody
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string Props { get; set; }
    }
}
