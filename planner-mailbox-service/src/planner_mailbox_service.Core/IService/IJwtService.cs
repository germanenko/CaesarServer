using planner_common_package.Entities;
using planner_mailbox_service.Core.Entities.Response;

namespace planner_mailbox_service.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}