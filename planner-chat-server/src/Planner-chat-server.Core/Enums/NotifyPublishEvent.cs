using System.Text.Json.Serialization;

namespace Planner_chat_server.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NotifyPublishEvent
    {
        AddAccountToChat,
        ResponseTaskChat,
        MessageSentToChat
    }
}