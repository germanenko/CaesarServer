using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using planner_chat_service.Core.IService;
using planner_common_package.Enums;
using System.Net.Mime;
using System.Text;

namespace planner_chat_service.Infrastructure.Service
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://planner-notify-service:80/api/")
            };
        }


        public async Task<bool> SendNotification(Guid userId, string title, string content, NotificationType type, Dictionary<string, string>? data = null)
        {

            try
            {
                Dictionary<string, string> baseData = new Dictionary<string, string>()
                {
                    { "type", type.ToString() }
                };

                var mergedData = MergeDictionaries(baseData, data, false);

                var s = JsonConvert.SerializeObject(mergedData);

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

        public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(
            Dictionary<TKey, TValue> first,
            Dictionary<TKey, TValue>? second,
            bool overwrite = true) where TKey : notnull
        {
            var result = new Dictionary<TKey, TValue>(first);

            if (second == null)
                return result;

            foreach (var item in second)
            {
                if (overwrite || !result.ContainsKey(item.Key))
                {
                    result[item.Key] = item.Value;
                }
            }

            return result;
        }
    }
}
