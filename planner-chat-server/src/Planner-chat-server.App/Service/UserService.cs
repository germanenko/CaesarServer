using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.IService;

namespace Planner_chat_server.App.Service
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly HttpClient _httpClient;

        public UserService(ILogger<UserService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("AuthService"); 
        }

        public async Task<string> GetUserName(Guid userId)
        {

            _logger.LogInformation("🔍 Getting user name for {UserId}", userId);
            var response = await _httpClient.GetAsync($"user/{userId}");
          

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<ProfileBody>(content);
                var userName = user?.Nickname ?? userId.ToString();

                return userName;
            }
            else 
            {
                _logger.LogInformation($"Response: {await response.Content.ReadAsStringAsync()}");
            }

            return userId.ToString();
        }
    }
}
