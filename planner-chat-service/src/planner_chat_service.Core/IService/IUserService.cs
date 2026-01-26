using planner_client_package.Entities;

namespace planner_chat_service.Core.IService
{
    public interface IUserService
    {
        Task<ProfileBody?> GetUserData(Guid userId);
    }
}
