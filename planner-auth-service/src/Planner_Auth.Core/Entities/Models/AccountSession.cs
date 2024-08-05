namespace Planner_Auth.Core.Entities.Models
{
    public class AccountSession
    {
        public Guid Id { get; set; }
        public string DeviceId { get; set; }

        public Account Account { get; set; }
        public Guid AccountId { get; set; }

        public string? Token { get; set; }
        public DateTime? TokenValidBefore { get; set; }
    }
}