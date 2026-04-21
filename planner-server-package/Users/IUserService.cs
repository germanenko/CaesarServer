using planner_client_package.Entities;
using System;
using System.Threading.Tasks;

namespace planner_server_package.Users
{
    public interface IUserService
    {
        Task<ProfileBody> GetUserData(Guid userId);
        Task<ProfileBody> GetUserData(string identifier);
    }
}
