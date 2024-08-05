using Planer_mailbox_service.Core.Entities.Response;

namespace Planer_mailbox_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}