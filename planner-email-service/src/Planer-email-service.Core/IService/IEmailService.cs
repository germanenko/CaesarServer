using Planer_email_service.Core.Entities;

namespace Planer_email_service.Core.IService
{
    public interface IEmailService
    {
        Task<ServiceResponse<string>> SendMessage(string email, string subject, string message);
        Task<ServiceResponse<string>> SendMessage(string fromEmail, string toEmail, string subject, string message);
    }
}