using System.Text.Json.Serialization;

namespace planner_common_package.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageType
    {
        Text,
        File,
        Mail
    }
}