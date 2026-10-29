using planner_common_package.Enums;
using planner_mailbox_service.Core.Entities.Models;

namespace planner_mailbox_service.Core.IService
{
    public interface ITokenService
    {
        Task<(string AccessToken, string RefreshToken)?> GetUpdatedTokens(AccountMailCredentials emailCredentials, EmailProvider emailProvider);
    }
}