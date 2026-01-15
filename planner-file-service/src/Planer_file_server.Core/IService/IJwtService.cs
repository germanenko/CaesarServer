using Planer_file_server.Core.Entities.Request;

namespace Planer_file_server.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}