using Microsoft.Extensions.Logging;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.IService;
using System.Text.Json;

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
            try
            {
                _logger.LogInformation("🔍 Getting user name for {UserId}", userId);
                var response = await _httpClient.GetAsync($"user/{userId}");

                _logger.LogInformation("📡 Response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("📦 Raw response: {Content}", content);

                    // 🔥 Используйте System.Text.Json вместо Newtonsoft.Json
                    var user = JsonSerializer.Deserialize<ProfileBody>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // 🔥 Важно!
                    });

                    var userName = user?.Nickname ?? userId.ToString();
                    _logger.LogInformation("✅ Got user name: {UserName}", userName);

                    return userName;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ HTTP {StatusCode}: {Error}", response.StatusCode, errorContent);
                }
            }
            catch (HttpRequestException ex) when (ex.InnerException is IOException)
            {
                _logger.LogError("💥 Connection closed by auth-service for {UserId}", userId);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "❌ JSON deserialization failed for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error for {UserId}", userId);
            }

            return userId.ToString();
        }
    }
}
