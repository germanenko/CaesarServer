using CaesarServerLibrary.Entities;

namespace Planer_task_board.Core.IService
{
    public interface IUserService
    {
        Task<ProfileBody?> GetUserData(Guid userId);
    }
}
