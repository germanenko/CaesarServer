using planner_email_service.Core.Entities;

namespace planner_email_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}