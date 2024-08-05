using Planer_email_service.Core.Entities.Response;

namespace Planer_email_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}