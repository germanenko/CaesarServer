using System.Text.Json.Serialization;

namespace Planner_Auth.Core.Entities.Response
{
    public class MailUserInfoBody
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }
    }
}