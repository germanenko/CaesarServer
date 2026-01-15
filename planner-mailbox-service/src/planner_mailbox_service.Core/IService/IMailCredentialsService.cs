using planner_mailbox_service.Core.Entities.Models;
using planner_mailbox_service.Core.Entities.Response;
using planner_mailbox_service.Core.Enums;

namespace planner_mailbox_service.Core.IService
{
    public interface IMailCredentialsService
    {
        Task<ServiceResponse<List<AccountMailCredentials>>> GetMailCredentials(Guid accountId);
        Task<ServiceResponse<AccountMailCredentials>> AddMailCredential(string email, string access_token, string refresh_token, Guid accountId, EmailProvider emailProvider);
        Task<ServiceResponse<AccountMailCredentials>> GetMailCredential(string email);
    }
}