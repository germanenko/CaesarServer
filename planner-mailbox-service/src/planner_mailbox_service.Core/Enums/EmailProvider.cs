using System.Text.Json.Serialization;

namespace planner_mailbox_service.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EmailProvider
    {
        Gmail = 1,
        MailRu = 2
    }
}