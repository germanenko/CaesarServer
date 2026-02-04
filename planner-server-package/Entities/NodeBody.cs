using planner_common_package;
using planner_common_package.Enums;
using planner_server_package.Interface;
using System;
using System.Text.Json.Serialization;

namespace planner_server_package.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator.TypeDiscriminatorPropertyName)]
    [JsonDerivedType(typeof(BoardBody), Discriminator.Board)]
    [JsonDerivedType(typeof(ColumnBody), Discriminator.Column)]
    [JsonDerivedType(typeof(TaskBody), Discriminator.Task)]
    [JsonDerivedType(typeof(ChatBody), Discriminator.Chat)]
    [JsonDerivedType(typeof(MessageBody), Discriminator.ChatMessage)]
    public class NodeBody : ISyncable
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string Props { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public NodeLinkBody Link { get; set; }
        public AccessRightBody AccessRight { get; set; }
    }
}
