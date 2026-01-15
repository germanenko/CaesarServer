using CaesarServerLibrary.Entities;

namespace planner_chat_service.Core.IService
{
    public interface IUserService
    {
        Task<ProfileBody?> GetUserData(Guid userId);
    }
}
