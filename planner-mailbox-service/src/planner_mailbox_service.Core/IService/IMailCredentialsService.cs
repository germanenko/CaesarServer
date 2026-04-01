using planner_client_package.Entities;
using planner_mailbox_service.Core.Entities.Models;
using planner_mailbox_service.Core.Enums;
using planner_server_package;

namespace planner_mailbox_service.Core.IService
{
    public interface IMailCredentialsService
    {
        Task<ServiceResponse<List<AccountMailCredentials>>> GetMailCredentials(Guid accountId);
        Task<ServiceResponse<AccountMailCredentials>> AddMailCredential(string email, string access_token, string refresh_token, Guid accountId, EmailProvider emailProvider);
        Task<ServiceResponse<AccountMailCredentials>> GetMailCredential(string email);
    }
}