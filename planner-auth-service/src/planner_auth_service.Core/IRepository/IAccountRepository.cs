using planner_server_package.Entities;
using planner_server_package.Enums;
using planner_auth_service.Core.Entities.Models;

namespace planner_auth_service.Core.IRepository
{
    public interface IAccountRepository
    {
        Task<Account?> AddAsync(string identifier, string nickname, string password, string deviceId, DeviceTypeId deviceTypeId, DefaultAuthenticationMethod authenticationMethod, string role, AuthenticationProvider providerType);
        Task<AccountSession?> GetSessionAsync(Guid accountId, string deviceId);
        Task<AccountSession?> GetSessionAsync(Guid sessionId);
        Task<AccountSession?> AddSessionAsync(string deviceId, Account account);
        Task<List<AccountSession>> GetSessionsAsync(Guid accountId);
        Task<Account?> GetAsync(Guid id);
        Task<Account?> GetAccountByTagAsync(string accountTag);
        Task<List<Account>> GetAccountsByPatternAccountTag(string patternAccountTag);
        Task<List<Account>> GetAccountsAsync(List<string> identifiers);
        Task<List<Account>> GetAccountsAsync(IEnumerable<Guid> ids);
        Task<List<Account>> GetAccountsByPatternIdentifier(string identifier);
        Task<Account?> UpdateAccountTagAsync(Guid id, string accountTag);
        Task<Account?> UpdateAuthorizationProviderAsync(Guid id, string authorizationProvider);
        Task<Account?> GetAsync(string identifier);
        Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null);
        Task<AccountSession?> GetSessionByTokenAndAccount(string refreshTokenHash);
        Task<Account?> UpdateProfileIconAsync(Guid accountId, string filename);
        Task<GoogleToken?> AddAsync(GoogleTokenBody token, Guid accountId);
        Task<GoogleToken?> GetGoogleTokenAsync(Guid userId);
        Task<Account?> ChangePassword(Account account, string newPasswordHash);
        Task DeleteInvalidSessions();
    }
}