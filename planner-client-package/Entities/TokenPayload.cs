using System;

namespace planner_client_package.Entities
{
    public class TokenPayload
    {
        public Guid AccountId { get; set; }
        public Guid SessionId { get; set; }
        public string Role { get; set; }
    }
}