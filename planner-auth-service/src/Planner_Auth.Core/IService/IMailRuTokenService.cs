using Planner_Auth.Core.Entities.Response;

namespace Planner_Auth.Core.IService
{
    public interface IMailRuTokenService
    {
        string GetAuthorizationUrl();
        Task<MailruTokenResponse?> GetTokenAsync(string code);
        Task<AccessTokenWithLifetimeBody?> UpdateToken(string refresh_token);
        Task<MailUserInfoBody?> GetUserInfo(string accessToken);
    }
}