using CaesarServerLibrary.Entities;

namespace planner_node_service.Core.IService
{
    public interface IUserService
    {
        Task<ProfileBody?> GetUserData(Guid userId);
    }
}
