using planner_client_package.Entities;
using planner_common_package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace planner_client_package.Interface
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator.TypeDiscriminatorPropertyName)]
    [JsonDerivedType(typeof(BoardBody), Discriminator.Board)]
    [JsonDerivedType(typeof(ColumnBody), Discriminator.Column)]
    [JsonDerivedType(typeof(TaskBody), Discriminator.Task)]
    [JsonDerivedType(typeof(ChatBody), Discriminator.Chat)]
    [JsonDerivedType(typeof(MessageBody), Discriminator.ChatMessage)]
    [JsonDerivedType(typeof(NotificationSettingsBody), Discriminator.NotificationSettings)]
    [JsonDerivedType(typeof(ProfileBody), Discriminator.Profile)]
    public interface ISyncable
    {
    }
}
