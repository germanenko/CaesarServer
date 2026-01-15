namespace Planer_mailbox_service.Core.Entities.Models
{
    public class AccountMailCredentials
    {
        public Guid Id { get; set; }

        public string Email { get; set; }
        public string EmailProvider { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public Guid AccountId { get; set; }
    }
}