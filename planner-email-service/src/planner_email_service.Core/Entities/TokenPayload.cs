namespace planner_email_service.Core.Entities
{
    public class TokenPayload
    {
        public Guid AccountId { get; set; }
        public string Role { get; set; }
        public Guid SessionId { get; set; }
    }
}