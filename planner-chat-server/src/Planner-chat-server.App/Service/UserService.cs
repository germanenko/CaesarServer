using Microsoft.Extensions.Logging;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.IService;
using System;
using System.Collections.Concurrent;
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
        private readonly ILogger<UserService> _logger;
        private readonly HttpClient _httpClient;

        public UserService(ILogger<UserService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("AuthService"); // 🔥 Используем именованный клиент
        }

        public async Task<string> GetUserName(Guid userId)
        {
            try
            {
                _logger.LogInformation("🔍 Getting user name for {UserId}", userId);
                var response = await _httpClient.GetAsync($"user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<ProfileBody>(content);
                    var userName = user?.Nickname ?? userId.ToString();

                    return userName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user name for {UserId}", userId);
            }

            return userId.ToString();
        }
    }
}
