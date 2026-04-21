using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace planner_server_package.Users
{
    public interface IUserService
    {
        Task<ProfileBody> GetUserData(Guid userId);
        Task<List<ProfileBody>> GetUsersData(List<Guid> userIds);
        Task<ProfileBody> GetUserData(string identifier);
    }
}
