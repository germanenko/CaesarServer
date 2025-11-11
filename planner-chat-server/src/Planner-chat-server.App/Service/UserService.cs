using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UserService> _logger;
        private readonly HttpClient _httpClient;

        public UserService(ILogger<UserService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("AuthService");
        }

        public async Task<string> GetUserName(Guid userId)
        {
            const int maxRetries = 3;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    _logger.LogInformation("Getting user name for {UserId} (attempt {Retry})", userId, retry + 1);

                    var absoluteUrl = $"http://planner-auth-service:8888/api/user/{userId}";
                    var response = await _httpClient.PostAsync(absoluteUrl, null);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var user = JsonSerializer.Deserialize<ProfileBody>(content);
                        _logger.LogInformation("Successfully retrieved user name: {UserName}", user?.Nickname);
                        return user?.Nickname ?? userId.ToString();
                    }
                    else
                    {
                        _logger.LogWarning("HTTP {StatusCode} when getting user {UserId}", response.StatusCode, userId);

                        if (retry == maxRetries - 1)
                            return userId.ToString();

                        await Task.Delay(GetRetryDelay(retry));
                    }
                }
                catch (HttpRequestException ex) when (ex.InnerException is IOException)
                {
                    _logger.LogWarning("Connection prematurely closed for user {UserId}, retrying... (attempt {Retry})", userId, retry + 1);
                    await Task.Delay(GetRetryDelay(retry));
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    _logger.LogWarning("Timeout getting user {UserId}, retrying... (attempt {Retry})", userId, retry + 1);
                    await Task.Delay(GetRetryDelay(retry));
                }
                catch (Exception ex) when (retry < maxRetries - 1)
                {
                    _logger.LogWarning(ex, "Request failed for user {UserId}, retrying... (attempt {Retry})", userId, retry + 1);
                    await Task.Delay(GetRetryDelay(retry));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get user name for {UserId} after {RetryCount} retries", userId, retry + 1);
                    break;
                }
            }

            return userId.ToString();
        }

        private TimeSpan GetRetryDelay(int retryCount)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, retryCount));
        }
    }
}
