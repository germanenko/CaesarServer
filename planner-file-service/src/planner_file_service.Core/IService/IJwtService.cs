using planner_file_service.Core.Entities.Request;

namespace planner_file_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}