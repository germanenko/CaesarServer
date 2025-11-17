using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Planner_chat_server.Infrastructure.Service
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;          
        }


        public async Task<bool> SendNotification(Guid userId, string title, string content, Dictionary<string, string>? data = null)
        {

            try
            {
                var s = JsonConvert.SerializeObject(data);

                var body = new StringContent(s, Encoding.UTF8, MediaTypeNames.Application.Json);

                var url = $"sendFCMNotification?userId={userId}&title={title}&content={content}";

                var response = await _httpClient.PostAsync(url, body);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Уведомление успешно отправлено через HTTP");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ Ошибка при отправке уведомления. Status: {StatusCode}, Response: {ErrorContent}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Исключение при отправке уведомления через HTTP");
                return false;
            }
        }
    }
}
