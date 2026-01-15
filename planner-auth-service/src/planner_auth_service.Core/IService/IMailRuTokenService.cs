using planner_auth_service.Core.Entities.Response;

namespace planner_auth_service.Core.IService
{
    public interface IMailRuTokenService
    {
        string GetAuthorizationUrl();
        Task<MailruTokenResponse?> GetTokenAsync(string code);
        Task<AccessTokenWithLifetimeBody?> UpdateToken(string refresh_token);
        Task<MailUserInfoBody?> GetUserInfo(string accessToken);
    }
}