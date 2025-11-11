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
        private readonly ConcurrentDictionary<Guid, string> _cache = new();

        public Task<string> GetUserName(Guid userId)
        {
            var userName = _cache.GetOrAdd(userId, uid => uid.ToString());

            _ = TryUpdateUserNameAsync(userId);

            return Task.FromResult(userName);
        }

        private async Task TryUpdateUserNameAsync(Guid userId)
        {
            // Если уже есть нормальное имя (не ID), пропускаем
            if (_cache.TryGetValue(userId, out var current) && current != userId.ToString())
                return;

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

                // Добавляем заголовки
                client.DefaultRequestHeaders.Add("User-Agent", "ChatService");

                var response = await client.GetAsync($"http://planner-auth-service:8888/api/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<ProfileBody>(content);
                    var userName = user?.Nickname ?? userId.ToString();

                    _cache[userId] = userName;
                    _logger.LogInformation("✅ Cached user name for {UserId}: {Name}", userId, userName);
                }
            }
            catch (Exception ex)
            {
                // 🔥 ТИХИЙ fail - не логируем ошибки чтобы не засорять логи
                // Не пытаемся повторно - просто оставляем ID как имя
            }
        }
    }
}
