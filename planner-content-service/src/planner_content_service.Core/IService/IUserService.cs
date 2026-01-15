using CaesarServerLibrary.Entities;

namespace planner_content_service.Core.IService
{
    public interface IUserService
    {
        Task<ProfileBody?> GetUserData(Guid userId);
    }
}
