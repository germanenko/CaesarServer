using Microsoft.Extensions.Logging;
using planner_client_package.Entities;
using planner_common_package.Entities;
using planner_common_package.Enums;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace planner_server_package.Access
{
    public class AccessService : IAccessService
    {
        private readonly ILogger<AccessService> _logger;
        private readonly HttpClient _httpClient;

        public AccessService(ILogger<AccessService> logger)
        {
            _logger = logger;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://planner-node-service:8080/api/")
            };
        }

        public async Task<bool> CheckAccess(Guid accountId, Guid nodeId, Permission minRequiredPermission)
        {
            try
            {
                var response = await _httpClient.GetAsync($"checkAccess?accountId={accountId}&nodeId={nodeId}&minRequiredPermission={minRequiredPermission}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"Access checked = {content}");

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var hasAccess = JsonSerializer.Deserialize<Response<bool>>(content, options);

                    return hasAccess.Body;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("HTTP {StatusCode}: {Error}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error");
            }

            return false;
        }
    }
}
