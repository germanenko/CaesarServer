using Planer_mailbox_service.Core.Entities.Response;
using Planer_mailbox_service.Core.Enums;

namespace Planer_mailbox_service.Core.IService
{
    public interface IMailboxService
    {
        Task<ServiceResponse<string>> SendMessage(string fromEmail, string senderName, string toEmail, string toName, string subject, string message, string password, EmailProvider emailProvider);
        Task<ServiceResponse<List<EmailMessageInfoDto>>> GetMessages(string email, string access_token, int offset, int count, EmailProvider emailProvider);
        Task<ServiceResponse<string>> DeleteMessages(string email, string accessToken, int messageIndex, EmailProvider emailProvider);
    }
}