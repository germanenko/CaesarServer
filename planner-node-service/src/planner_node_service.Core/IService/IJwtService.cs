using CaesarServerLibrary.Entities;

namespace planner_node_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}