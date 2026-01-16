using planner_server_package.Entities;

namespace planner_chat_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}