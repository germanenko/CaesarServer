using System.Text.Json.Serialization;

namespace Planer_mailbox_service.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmailProvider
    {
        Gmail = 1,
        MailRu = 2
    }
}