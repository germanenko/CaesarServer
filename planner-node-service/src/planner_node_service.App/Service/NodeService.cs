using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using planner_client_package.Entities;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.Interface;
using planner_server_package.RabbitMQ;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using static NpgsqlTypes.NpgsqlTsQuery;

namespace planner_node_service.App.Service
{
    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IScopeRepository _scopeRepository;
        private readonly IAccessRepository _accessRepository;
        private readonly ILogRepository _logRepository;
        private readonly IPublisherService _publisherService;
        private readonly ILogger<NodeService> _logger;

        public NodeService(
            INodeRepository nodeRepository, IAccessRepository accessRepository, IPublisherService notifyService, ILogRepository historyRepository, ILogger<NodeService> logger)
        {
            _nodeRepository = nodeRepository;
            _accessRepository = accessRepository;
            _publisherService = notifyService;
            _logRepository = historyRepository;
            _logger = logger;
        }

        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodes(Guid accountId)
        {
            var nodes = await _nodeRepository.GetNodes(accountId);

            var bodies = nodes.Select(x => x.ToNodeBody()).ToList();

            var result = new List<NodeBody>();

            result.AddRange(await GetContentNodesByIdAsync(bodies));
            result.AddRange(await GetChatNodesByIdAsync(bodies));

            foreach (var body in bodies)
            {
                var history = await _logRepository.GetLastHistory(body.Id);
                body.UpdatedBy = history.UpdatedById;
                body.UpdatedAt = history.UpdatedAt;
            }

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }


        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetScopes(Guid accountId)
        {
            var nodes = await _scopeRepository.GetScopes(accountId);

            var bodies = nodes.Select(x => x.ToNodeBody()).ToList();

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = bodies
            };
        }


        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodesByIds(Guid accountId, List<Guid> nodeIds)
        {
            var nodes = new List<NodeBody>();
            foreach (var id in nodeIds)
            {
                if (await _accessRepository.CheckAccess(accountId, id, Permission.Read))
                {
                    nodes.Add((await _nodeRepository.GetNode(id)).ToNodeBody());
                }
            }

            var result = new List<NodeBody>();

            result.AddRange(await GetContentNodesByIdAsync(nodes));
            result.AddRange(await GetChatNodesByIdAsync(nodes));

            foreach (var body in nodes)
            {
                var history = await _logRepository.GetLastHistory(body.Id);
                body.UpdatedBy = history.UpdatedById;
                body.UpdatedAt = history.UpdatedAt;
            }

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<IEnumerable<NodeLinkBody>>> GetNodeLinks(Guid accountId)
        {
            var nodeLinks = await _nodeRepository.GetNodesLinks(accountId);

            return new ServiceResponse<IEnumerable<NodeLinkBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeLinks?
                  .Where(x => x != null)
                  .Select(x => x.ToBody())
                  .ToList()!
               ?? new List<NodeLinkBody>()
            };
        }

        public async Task<ServiceResponse<NodeBody>> AddScope(NodeBody nodeBody)
        {
            if (nodeBody.Name.IsNullOrEmpty())
            {
                return new ServiceResponse<NodeBody>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Errors = new[] { "Name field is reqiured" }
                };
            }

            var newNode = await _nodeRepository.AddScope(nodeBody);

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNode
            };
        }

        public async Task<ServiceResponse<NodeBody>> AddOrUpdateNode(NodeBody nodeBody)
        {
            if (nodeBody.Name.IsNullOrEmpty())
            {
                return new ServiceResponse<NodeBody>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Errors = new[] { "Name field is reqiured" }
                };
            }

            var newNode = await _nodeRepository.AddOrUpdateNode(nodeBody);

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNode
            };
        }

        public async Task<ServiceResponse<bool>> DeleteNode(Guid accountId, Guid nodeId)
        {
            if (await _nodeRepository.GetNode(nodeId) == null)
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Errors = new[] { "Нет ноды" }
                };
            }

            if (await _accessRepository.CheckAccess(accountId, nodeId, Permission.Write) == false)
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            var newNode = await _nodeRepository.DeleteNode(accountId, nodeId);

            return new ServiceResponse<bool>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNode
            };
        }

        public async Task<ServiceResponse<NodeLink>> AddOrUpdateNodeLink(NodeLinkBody node)
        {
            var nodeLink = await _nodeRepository.AddOrUpdateNodeLink(node);

            return new ServiceResponse<NodeLink>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeLink
            };
        }

        public async Task<ServiceResponse<List<NodeLink>>> AddOrUpdateNodeLinks(List<NodeLinkBody> nodes)
        {
            var nodeLink = await _nodeRepository.AddOrUpdateNodeLinks(nodes);

            return new ServiceResponse<List<NodeLink>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeLink
            };
        }


        public async Task<ServiceResponse<List<EntityVersionBody>>> GetManifest(Guid accountId)
        {
            var nodes = await GetNodes(accountId);

            if (nodes.Body.IsNullOrEmpty())
            {
                return new ServiceResponse<List<EntityVersionBody>>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Errors = new[] { "Ноды отсутствуют" }
                };
            }

            List<EntityVersionBody> logs = new List<EntityVersionBody>();
            foreach (var item in nodes.Body)
            {
                var log = await _logRepository.GetLastLogForEntity(item.Id);

                if (log != null)
                {
                    var entityVersion = new EntityVersionBody()
                    {
                        EntityId = log.EntityId,
                        Version = item.Version
                    };

                    logs.Add(entityVersion);
                }
            }

            foreach (var item in logs)
            {
                _logger.LogInformation($"Log: {item.EntityId} - {item.Version}");
            }

            return new ServiceResponse<List<EntityVersionBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = logs
            };
        }


        public async Task<ServiceResponse<List<EntityVersionBody>>> GetScopesManifest(Guid accountId)
        {
            var nodes = await GetScopes(accountId);

            if (nodes.Body.IsNullOrEmpty())
            {
                return new ServiceResponse<List<EntityVersionBody>>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Errors = new[] { "Scopes отсутствуют" }
                };
            }

            List<EntityVersionBody> logs = new List<EntityVersionBody>();
            foreach (var item in nodes.Body)
            {
                var log = await _logRepository.GetLastLogForEntity(item.Id);

                if (log != null)
                {
                    var entityVersion = new EntityVersionBody()
                    {
                        EntityId = log.EntityId,
                        Version = log.ScopeVersion
                    };

                    logs.Add(entityVersion);
                }
            }

            foreach (var item in logs)
            {
                _logger.LogInformation($"Log: {item.EntityId} - {item.Version}");
            }

            return new ServiceResponse<List<EntityVersionBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = logs
            };
        }

        public async Task<ServiceResponse<List<NodeBody>>> LoadNodes(List<NodeBody> nodeBodies, TokenPayload tokenPayload)
        {
            var newNodes = await _nodeRepository.AddOrUpdateNodes(nodeBodies);

            List<ISyncable> contentBodies = new List<ISyncable>();
            List<ISyncable> chatBodies = new List<ISyncable>();

            contentBodies.AddRange(nodeBodies.OfType<BoardBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            contentBodies.AddRange(nodeBodies.OfType<ColumnBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            contentBodies.AddRange(nodeBodies.OfType<TaskBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());

            chatBodies.AddRange(nodeBodies.OfType<ChatBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            chatBodies.AddRange(nodeBodies.OfType<MessageBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            chatBodies.AddRange(nodeBodies.OfType<ChatSettingsBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());

            SyncEntitiesEvent contentNodesEvent = new SyncEntitiesEvent()
            {
                TokenPayload = tokenPayload,
                Bodies = contentBodies
            };

            SyncEntitiesEvent chatNodesEvent = new SyncEntitiesEvent()
            {
                TokenPayload = tokenPayload,
                Bodies = contentBodies
            };

            _publisherService.Publish(contentNodesEvent, PublishEvent.ContentNodes);
            _publisherService.Publish(chatNodesEvent, PublishEvent.ChatNodes);

            return new ServiceResponse<List<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeBodies
            };
        }

        public async Task<List<NodeBody>> GetContentNodesByIdAsync(List<NodeBody> nodes)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://planner-content-service:80/api/"),
            };

            var nodeIds = nodes.Select(x => x.Id);

            var queryString = string.Join("&", nodeIds.Select(id => $"nodeIds={id}"));

            var response = await client.GetAsync($"content/getNodesByIds?{queryString}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(resultString);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var result = JsonSerializer.Deserialize<List<NodeBody>>(resultString, options);

                return result;
            }
            else
            {
                _logger.LogInformation($"Error: {response.StatusCode}");
                return nodes;
            }
        }

        public async Task<List<NodeBody>> GetChatNodesByIdAsync(List<NodeBody> nodes)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://planner-chat-service:80/api/"),
            };

            var nodeIds = nodes.Select(x => x.Id);

            var queryString = string.Join("&", nodeIds.Select(id => $"nodeIds={id}"));

            var response = await client.GetAsync($"chat/getNodesByIds?{queryString}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(resultString);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var result = JsonSerializer.Deserialize<List<NodeBody>>(resultString, options);

                return result;
            }
            else
            {
                _logger.LogInformation($"Error: {response.StatusCode}");
                return nodes;
            }
        }
    }
}

