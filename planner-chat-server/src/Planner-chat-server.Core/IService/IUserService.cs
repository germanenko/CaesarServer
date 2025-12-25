using CaesarServerLibrary.Entities;

namespace Planner_chat_server.Core.IService
{
    public interface IUserService
    {
        Task<ProfileBody?> GetUserData(Guid userId);
    }
}
