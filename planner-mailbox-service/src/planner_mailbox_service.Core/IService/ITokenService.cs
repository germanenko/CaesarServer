using planner_mailbox_service.Core.Entities.Models;
using planner_mailbox_service.Core.Enums;

namespace planner_mailbox_service.Core.IService
{
    public interface ITokenService
    {
        Task<(string AccessToken, string RefreshToken)?> GetUpdatedTokens(AccountMailCredentials emailCredentials, EmailProvider emailProvider);
    }
}