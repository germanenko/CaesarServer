using System.Text.Json.Serialization;

namespace CaesarServerLibrary.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Status
    {
        Undefined,
        Draft,
        Deleted,
        InProgress,
        Completed
    }
}