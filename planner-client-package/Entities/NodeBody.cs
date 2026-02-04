using planner_client_package.Interface;
using planner_common_package;
using planner_common_package.Enums;
using System;
using System.Text.Json.Serialization;

namespace planner_client_package.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator.TypeDiscriminatorPropertyName)]
    [JsonDerivedType(typeof(BoardBody), Discriminator.Board)]
    [JsonDerivedType(typeof(ColumnBody), Discriminator.Column)]
    [JsonDerivedType(typeof(TaskBody), Discriminator.Task)]
    [JsonDerivedType(typeof(ChatBody), Discriminator.Chat)]
    [JsonDerivedType(typeof(MessageBody), Discriminator.ChatMessage)]
    public class NodeBody : ISyncable, IBody
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string Props { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public NodeLinkBody Link { get; set; }
        public AccessRightBody AccessRight { get; set; }
    }
}
