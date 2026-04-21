using Microsoft.Extensions.Logging;
using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace planner_server_package.Users
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

        public async Task<ProfileBody> GetUserData(Guid userId)
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

                    return user;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("HTTP {StatusCode}: {Error}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error for {UserId}", userId);
            }

            return null;
        }

        public async Task<ProfileBody> GetUserData(string identifier)
        {
            try
            {
                var response = await _httpClient.GetAsync($"users/identifier?identifierPattern={identifier}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var user = JsonSerializer.Deserialize<List<ProfileBody>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return user.FirstOrDefault();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"HTTP {response.StatusCode}: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error for {identifier}");
            }

            return null;
        }
    }
}
