using planner_server_package.Entities;

namespace planner_auth_service.Core.Entities.Models
{
    public class GoogleToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public GoogleTokenBody ToBody()
        {
            return new GoogleTokenBody()
            {
                AccountId = UserId,
                AccessToken = AccessToken,
                RefreshToken = RefreshToken
            };
        }
    }
}