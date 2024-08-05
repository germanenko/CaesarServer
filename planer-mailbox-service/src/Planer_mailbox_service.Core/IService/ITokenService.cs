using Planer_mailbox_service.Core.Entities.Models;
using Planer_mailbox_service.Core.Enums;

namespace Planer_mailbox_service.Core.IService
{
    public interface ITokenService
    {
        Task<(string AccessToken, string RefreshToken)?> GetUpdatedTokens(AccountMailCredentials emailCredentials, EmailProvider emailProvider);
    }
}