namespace planner_auth_service.Core.Entities.Response
{
    public class PasswordResetTokenPayload
    {
        public Guid AccountId { get; set; }
        public string TokenId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
