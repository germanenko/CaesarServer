using CaesarServerLibrary.Entities;

namespace Planer_task_board.Core.IService
{
    public interface IJwtService
    {
        TokenPayload GetTokenPayload(string token);
    }
}