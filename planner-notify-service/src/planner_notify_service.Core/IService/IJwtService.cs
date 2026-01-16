using planner_notify_service.Core.Entities.Response;
using planner_server_package.Entities;

namespace planner_notify_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}