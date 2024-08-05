using Planner_chat_server.Core.Entities.Request;

namespace Planner_chat_server.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}