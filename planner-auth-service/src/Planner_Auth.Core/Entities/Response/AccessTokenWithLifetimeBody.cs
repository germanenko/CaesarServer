using System.Text.Json.Serialization;

namespace Planner_Auth.Core.Entities.Response
{
    public class AccessTokenWithLifetimeBody
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}