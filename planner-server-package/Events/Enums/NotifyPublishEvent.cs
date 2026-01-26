using System.Text.Json.Serialization;

namespace planner_server_package.Events.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NotifyPublishEvent
    {
        CreatePersonalChat,
        AddAccountToChat,
        ResponseTaskChat,
        MessageSentToChat
    }
}