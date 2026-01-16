using System.Text.Json.Serialization;

namespace planner_server_package.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AuthenticationProvider
    {
        Default,
        Google,
        MailRu
    }
}