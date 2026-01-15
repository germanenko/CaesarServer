using System.Text.Json.Serialization;

namespace CaesarServerLibrary.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmailProvider
    {
        Gmail,
        MailRu
    }
}