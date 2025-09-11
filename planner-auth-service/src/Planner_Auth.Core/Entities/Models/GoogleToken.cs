namespace Planner_Auth.Core.Entities.Models
{
    public class GoogleToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}