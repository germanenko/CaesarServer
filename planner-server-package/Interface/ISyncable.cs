using planner_common_package;
using planner_server_package.Entities;
using System;
using System.Text.Json.Serialization;

namespace planner_server_package.Interface
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator.TypeDiscriminatorPropertyName)]
    [JsonDerivedType(typeof(BoardBody), Discriminator.Board)]
    [JsonDerivedType(typeof(ColumnBody), Discriminator.Column)]
    [JsonDerivedType(typeof(JobBody), Discriminator.Task)]
    [JsonDerivedType(typeof(ChatBody), Discriminator.Chat)]
    [JsonDerivedType(typeof(MessageBody), Discriminator.ChatMessage)]
    public interface ISyncable
    {
    }
}
