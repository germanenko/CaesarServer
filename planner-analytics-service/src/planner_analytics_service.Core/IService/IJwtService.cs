using planner_common_package.Entities;

namespace planner_analytics_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}