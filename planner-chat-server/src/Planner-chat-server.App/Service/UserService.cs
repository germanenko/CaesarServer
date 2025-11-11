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
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserService> _logger;

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(30),
                BaseAddress = new Uri("http://127.0.0.1:8888/api/")
            };
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
                    _logger.LogInformation("✅ Response received: {Content}", content);

                    var user = JsonSerializer.Deserialize<ProfileBody>(content);
                    return user?.Nickname ?? userId.ToString();
                }
                else
                {
                    _logger.LogWarning("❌ HTTP {StatusCode} for user {UserId}", response.StatusCode, userId);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("⏰ Request timeout for user {UserId}", userId);
            }
            catch (HttpRequestException ex) when (ex.InnerException is IOException)
            {
                _logger.LogError("💥 Connection prematurely closed for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to get user name for {UserId}", userId);
            }

            return userId.ToString();
        }
    }
}
