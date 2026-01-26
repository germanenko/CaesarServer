using planner_common_package.Enums;
using System;

namespace planner_server_package.Events
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