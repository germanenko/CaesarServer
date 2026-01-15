using Planer_mailbox_service.Core.Entities.Response;

namespace Planer_mailbox_service.Core.IService
{
    public interface IGoogleTokenService
    {
        Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken);
    }
}