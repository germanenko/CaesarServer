using Microsoft.Extensions.Logging;
using planner_client_package.Entities;
using planner_server_package.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace planner_server_package.Node
{
    public class NodeService : INodeService
    {
        private readonly ILogger<NodeService> _logger;
        private readonly HttpClient _httpClient;

        public NodeService(ILogger<NodeService> logger)
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

        public async Task<ServiceResponse<NodeBody>> CreateOrUpdateNode(Guid accountId, NodeBody nodeBody)
        {
            try
            {
                var s = JsonSerializer.Serialize(nodeBody);

                var content = new StringContent(s, Encoding.UTF8, MediaTypeNames.Application.Json);

                var response = await _httpClient.PostAsync($"createOrUpdateNode?accountId={accountId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation(result);

                    var node = JsonSerializer.Deserialize<ServiceResponse<NodeBody>>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return node;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("HTTP {StatusCode}: {Error}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
            }

            return null;
        }
    }
}
