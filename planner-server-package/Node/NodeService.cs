using Microsoft.Extensions.Logging;
using planner_client_package.Entities;
using planner_common_package;
using planner_common_package.Entities;
using planner_common_package.Enums;
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

        public async Task<ServiceResponse<NodeBody>> CreateOrUpdateNode(Guid accountId, Guid requestId, NodeBody nodeBody, OperationName operationName)
        {
            try
            {
                var s = JsonSerializer.Serialize(nodeBody);

                var content = new StringContent(s, Encoding.UTF8, MediaTypeNames.Application.Json);

                _httpClient.DefaultRequestHeaders.Add(RequestHeader.RequestId, requestId.ToString());

                var response = await _httpClient.PostAsync($"createOrUpdateNode?accountId={accountId}&operationName={operationName}", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation(result);

                    var node = JsonSerializer.Deserialize<Response<ServiceResponse<NodeBody>>>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return node.Body;
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
