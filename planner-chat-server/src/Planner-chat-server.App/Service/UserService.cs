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

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://planner-auth-service:8888/api/")
            };
        }

        public async Task<string> GetUserName(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var user = JsonSerializer.Deserialize<ProfileBody>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var userName = user?.Nickname ?? userId.ToString();

                    return userName;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ HTTP {StatusCode}: {Error}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error for {UserId}", userId);
            }

            return userId.ToString();
        }
    }
}
