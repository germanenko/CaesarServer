using Planner_Auth.Core.Enums;

namespace Planner_Auth.Core.Entities.Events
{
    public class CreateAccountMailCredentialsEvent
    {
        public string Email { get; set; }
        public EmailProvider EmailProvider { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public Guid AccountId { get; set; }
    }
}