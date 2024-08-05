using Planer_mailbox_service.Core.Entities.Response;

namespace Planer_mailbox_service.Core.IService
{
    public interface IMailRuTokenService
    {
        string GetAuthorizationUrl();
        Task<MailruTokenResponse?> GetTokenAsync(string code);
        Task<AccessTokenWithLifetimeBody?> UpdateToken(string refresh_token);
        Task<MailUserInfoBody?> GetUserInfo(string accessToken);
    }
}