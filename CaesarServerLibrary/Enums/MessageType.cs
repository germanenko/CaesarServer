using System.Text.Json.Serialization;

namespace CaesarServerLibrary.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageType
    {
        Text,
        File,
        Mail
    }
}