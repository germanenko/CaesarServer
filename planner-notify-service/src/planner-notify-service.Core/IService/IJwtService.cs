using planner_notify_service.Core.Entities.Response;

namespace planner_notify_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}