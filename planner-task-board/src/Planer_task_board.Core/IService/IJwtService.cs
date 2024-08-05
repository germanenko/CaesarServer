using Planer_task_board.Core.Entities.Request;

namespace Planer_task_board.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}