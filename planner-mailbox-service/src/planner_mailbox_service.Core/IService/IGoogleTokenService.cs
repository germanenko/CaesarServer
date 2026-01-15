using planner_mailbox_service.Core.Entities.Response;

namespace planner_mailbox_service.Core.IService
{
    public interface IGoogleTokenService
    {
        Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken);
    }
}