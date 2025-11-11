using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Planner_chat_server.App.Service
{
    public class UserService : IUserService
    {
        public async Task<string> GetUserName(Guid userId)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://planner-auth-service:8888/api/"),
            };

            var response = await client.PostAsync("user/" + userId, null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var user = JsonSerializer.Deserialize<ProfileBody>(await response.Content.ReadAsStringAsync());
                return user.Nickname;
            }
            else
            {
                return userId.ToString();
            }
        }
    }
}
