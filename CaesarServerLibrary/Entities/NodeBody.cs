using CaesarServerLibrary.Enums;
using System;
using System.Data.Common;
using System.Text.Json.Serialization;

namespace CaesarServerLibrary.Entities
{
    [JsonDerivedType(typeof(CreateBoardBody), "board")]
    [JsonDerivedType(typeof(CreateColumnBody), "column")]
    [JsonDerivedType(typeof(CreateOrUpdateTaskBody), "task")]
    [JsonDerivedType(typeof(ChatBody), "chat")]
    [JsonDerivedType(typeof(MessageBody), "chatMessage")]
    public class NodeBody
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string Props { get; set; }
    }
}
