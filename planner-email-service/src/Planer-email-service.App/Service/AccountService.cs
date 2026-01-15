using System.Text.Json;
using Microsoft.Extensions.Logging;
using Planer_email_service.Core.Entities;
using Planer_email_service.Core.IService;

namespace Planer_email_service.App.Service
{
    public class AccountService : IAccountService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _client;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            string baseUrl,
            ILogger<AccountService> logger)
        {
            _baseUrl = baseUrl;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _client = new HttpClient(handler);
            _logger = logger;
        }

        public async Task<AccountDto?> GetAccountAsync(Guid id)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/api/user/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AccountDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                _logger.LogWarning("Request failed with status code: {StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account data for ID: {AccountId}", id);
            }
            return null;
        }
    }
}