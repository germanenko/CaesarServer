using Planer_mailbox_service.Core.Entities.Models;
using Planer_mailbox_service.Core.Entities.Response;
using Planer_mailbox_service.Core.Enums;

namespace Planer_mailbox_service.Core.IService
{
    public interface IMailCredentialsService
    {
        Task<ServiceResponse<List<AccountMailCredentials>>> GetMailCredentials(Guid accountId);
        Task<ServiceResponse<AccountMailCredentials>> AddMailCredential(string email, string access_token, string refresh_token, Guid accountId, EmailProvider emailProvider);
        Task<ServiceResponse<AccountMailCredentials>> GetMailCredential(string email);
    }
}