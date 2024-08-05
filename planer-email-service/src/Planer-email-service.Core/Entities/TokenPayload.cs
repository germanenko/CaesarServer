namespace Planer_email_service.Core.Entities.Response
{
    public class TokenPayload
    {
        public Guid AccountId { get; set; }
        public string Role { get; set; }
        public Guid SessionId { get; set; }
    }
}