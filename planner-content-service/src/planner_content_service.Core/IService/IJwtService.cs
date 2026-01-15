using CaesarServerLibrary.Entities;

namespace planner_content_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}