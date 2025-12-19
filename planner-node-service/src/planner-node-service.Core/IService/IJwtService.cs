using planner_node_service.Core.Entities.Response;

namespace planner_node_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}