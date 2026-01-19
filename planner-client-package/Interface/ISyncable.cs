using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace planner_client_package.Interface
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(BoardBody), "board")]
    [JsonDerivedType(typeof(ColumnBody), "column")]
    [JsonDerivedType(typeof(TaskBody), "task")]
    [JsonDerivedType(typeof(ChatBody), "chat")]
    [JsonDerivedType(typeof(MessageBody), "chatMessage")]
    [JsonDerivedType(typeof(NotificationSettingsBody), "notificationSettings")]
    [JsonDerivedType(typeof(ProfileBody), "profile")]
    public interface ISyncable
    {
    }
}
