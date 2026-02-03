using planner_common_package.Entities;
using planner_server_package.Entities;

namespace planner_content_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}