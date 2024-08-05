using System.Text.Json.Serialization;
using Planer_mailbox_service.Core.Enums;

namespace Planer_mailbox_service.Core.Entities.Request
{
    public class AccountMailCredentialsDto
    {
        public string Email { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EmailProvider EmailProvider { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public Guid AccountId { get; set; }
    }
}