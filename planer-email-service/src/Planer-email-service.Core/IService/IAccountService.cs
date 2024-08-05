using Planer_email_service.Core.Entities;

namespace Planer_email_service.Core.IService
{
    public interface IAccountService
    {
        Task<AccountDto?> GetAccountAsync(Guid id);
    }
}