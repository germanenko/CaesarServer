using planner_common_package.Entities;

namespace planner_file_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}