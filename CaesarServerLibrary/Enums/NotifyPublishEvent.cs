using System.Text.Json.Serialization;

namespace CaesarServerLibrary.Enums
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