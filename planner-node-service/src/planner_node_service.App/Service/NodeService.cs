using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using planner_client_package.Entities;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using planner_server_package;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.Interface;
using planner_server_package.RabbitMQ;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            INodeRepository nodeRepository, IAccessRepository accessRepository, IPublisherService notifyService, ILogRepository historyRepository, ILogger<NodeService> logger, IScopeRepository scopeRepository)
        {
            _nodeRepository = nodeRepository;
            _accessRepository = accessRepository;
            _publisherService = notifyService;
            _logRepository = historyRepository;
            _scopeRepository = scopeRepository;
            _logger = logger;
        }

        // Получение нод, принадлежащих данным rootIds (или всех нод, если rootIds не указаны), с учетом доступа и дополнительной информации из других сервисов
        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodes(Guid accountId, List<Guid>? rootIds = null)
        {
            // Получаем ноды из репозитория по данным rootIds (или все ноды, если rootIds не указаны)
            var nodes = await _nodeRepository.GetNodes(accountId, rootIds);

            if (nodes == null)
                return new ServiceResponse<IEnumerable<NodeBody>>()
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.OK
                };

            var bodies = nodes.Select(x => x.ToNodeBody()).ToList();

            // Список для хранения нод, по которым нужно сделать запросы в другие сервисы
            var requestBodies = new List<NodeBody>();

            // Список для хранения финальных результатов, которые будут возвращены клиенту
            var result = new List<NodeBody>();

            // Проверяем минимальный уровень доступа Read к каждой ноде и распределяем ноды между результатом и запросами к другим сервисам
            foreach (var body in bodies)
            {
                var hasAccess = await _accessRepository.CheckAccess(accountId, body.Id, Permission.Read);
                if (!hasAccess)
                {
                    result.Add(body);
                }
                else
                {
                    requestBodies.Add(body);
                }
            }

            // Получаем полные тела нод из других сервисов для тех нод и добавляем их в результат
            result.AddRange(await GetNodesFromDomainServices(accountId, requestBodies));

            // Заполняем ноды базовой информацией
            result = await FillNodes(accountId, result);

            result = result.DistinctBy(x => x.Id).ToList();

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }


        // Получение нод по Id с учетом доступа и дополнительной информации из других сервисов
        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodesByIds(Guid accountId, List<Guid> nodeIds)
        {
            _logger.LogInformation($"GetNodesByIds called with accountId: {accountId} and nodeIds: {string.Join(", ", nodeIds)}");

            if (nodeIds.IsNullOrEmpty())
            {
                return new ServiceResponse<IEnumerable<NodeBody>>()
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var nodes = new List<NodeBody>();

            foreach (var nodeId in nodeIds)
            {
                // Проверяем доступ к ноде
                if (await _accessRepository.CheckAccess(accountId, nodeId, Permission.Meta))
                {
                    // Если доступ есть, получаем ноду
                    var node = await _nodeRepository.GetNode(nodeId);

                    if (node != null)
                    {
                        // Добавляем ноду в результат
                        nodes.Add(node.ToNodeBody());

                        // Получаем Scope ноды и добавляем его в результат, если он существует и отличается от самой ноды
                        var scope = await _scopeRepository.GetNodeScope(nodeId);

                        if (scope != null && scope.Id != node.Id)
                            nodes.Add(scope.ToNodeBody());
                    }
                }
            }

            // Список для хранения нод, по которым нужно сделать запросы в другие сервисы
            var requestBodies = new List<NodeBody>();

            // Список для хранения финальных результатов, которые будут возвращены клиенту
            var result = new List<NodeBody>();

            nodes = nodes.DistinctBy(x => x.Id).ToList();

            // Проверяем минимальный уровень доступа Read к каждой ноде и распределяем ноды между результатом и запросами к другим сервисам
            foreach (var node in nodes)
            {
                var hasAccess = await _accessRepository.CheckAccess(accountId, node.Id, Permission.Read);
                if (!hasAccess)
                {
                    result.Add(node);
                }
                else
                {
                    requestBodies.Add(node);
                }
            }

            // Получаем полные тела нод из других сервисов для тех нод и добавляем их в результат
            result.AddRange(await GetNodesFromDomainServices(accountId, requestBodies));

            // Заполняем ноды базовой информацией
            result = await FillNodes(accountId, result);

            result = result.DistinctBy(x => x.Id).ToList();

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }


        // Добавление или обновление Scope
        public async Task<ServiceResponse<NodeBody>> AddOrUpdateScope(NodeBody nodeBody)
        {
            var newNode = await _nodeRepository.AddOrUpdateScope(nodeBody);

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNode
            };
        }


        // Добавление или обновление ноды
        public async Task<ServiceResponse<NodeBody>> AddOrUpdateNode(NodeBody nodeBody)
        {
            var newNode = await _nodeRepository.AddOrUpdateNode(nodeBody);

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNode
            };
        }


        // Удаление ноды
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


        // Изменение родителя ноды
        public async Task<ServiceResponse<NodeLinkBody>> ChangeNodeParent(Guid accountId, Guid nodeId, Guid newParentId)
        {
            // Проверяем, что нода и новый родитель существуют
            var node = await _nodeRepository.GetNode(nodeId);
            var parent = await _nodeRepository.GetNode(newParentId);

            if (node == null || parent == null)
            {
                return new ServiceResponse<NodeLinkBody>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Errors = new[] { "Нет ноды или нового родителя" }
                };
            }

            // Проверяем, что у пользователя есть права на запись как к ноде, так и к новому родителю
            var nodeAccess = _accessRepository.CheckAccess(accountId, nodeId, Permission.Write);
            var newParentAccess = _accessRepository.CheckAccess(accountId, newParentId, Permission.Write);

            if (nodeAccess == null || newParentAccess == null)
            {
                return new ServiceResponse<NodeLinkBody>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            // Получаем текущую связь ноды, чтобы понять, нужно ли ее создавать или изменять
            var link = await _nodeRepository.GetNodeLink(nodeId);
            var currentParent = link?.ParentNode;

            if (currentParent?.Id == newParentId)
            {
                return new ServiceResponse<NodeLinkBody>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Body = link?.ToBody()
                };
            }

            var nodeLink = new NodeLinkBody();

            // Если текущая связь есть, изменяем родителя, если нет - создаем новую связь
            if (currentParent != null)
            {
                nodeLink = await _nodeRepository.ChangeNodeParent(accountId, nodeId, newParentId);
            }
            else
            {
                nodeLink = (await _nodeRepository.AddOrUpdateNodeLink(new NodeLinkBody()
                {
                    ChildId = nodeId,
                    ParentId = newParentId,
                    RelationType = RelationType.Contains
                })).ToBody();
            }

            return new ServiceResponse<NodeLinkBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeLink
            };
        }


        // Получение манифеста нод по списку ScopeIds
        public async Task<ServiceResponse<List<EntityVersionBody>>> GetManifests(Guid accountId, List<Guid> scopeIds)
        {
            // Получаем ноды, которые принадлежат данным ScopeIds
            var nodes = await GetNodes(accountId, scopeIds);

            if (nodes.Body.IsNullOrEmpty())
            {
                return new ServiceResponse<List<EntityVersionBody>>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }

            // Создаем список для хранения информации о версиях нод, который будет возвращен клиенту
            List<EntityVersionBody> logs = new List<EntityVersionBody>();
            foreach (var item in nodes.Body!)
            {
                var entityVersion = new EntityVersionBody()
                {
                    EntityId = item.Id,
                    Version = item.Version
                };

                logs.Add(entityVersion);
            }

            return new ServiceResponse<List<EntityVersionBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = logs
            };
        }

        // Получение манифеста Scope'ов, к которым у пользователя есть доступ
        public async Task<ServiceResponse<List<EntityVersionBody>>> GetScopesManifest(Guid accountId)
        {
            // Получаем Scope'и, к которым у пользователя есть доступ
            var nodes = await _scopeRepository.GetScopes(accountId);

            var bodies = nodes.Select(x => x.ToNodeBody()).ToList();

            if (bodies.IsNullOrEmpty())
            {
                return new ServiceResponse<List<EntityVersionBody>>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }

            // Создаем список для хранения информации о версиях Scope'ов, который будет возвращен клиенту
            List<EntityVersionBody> logs = new List<EntityVersionBody>();
            foreach (var item in bodies!)
            {
                var log = await _logRepository.GetLastLogForScope(item.Id);

                if (log != null)
                {
                    var entityVersion = new EntityVersionBody()
                    {
                        EntityId = log.ScopeId,
                        Version = log.ScopeVersion
                    };

                    logs.Add(entityVersion);
                }
            }

            return new ServiceResponse<List<EntityVersionBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = logs
            };
        }


        // Получение полных тел нод из других сервисов 
        public async Task<List<NodeBody>> GetNodesFromDomainServices(Guid accountId, List<NodeBody> requestBodies)
        {
            var result = new List<NodeBody>();

            result.AddRange(await GetContentNodesByIdAsync(requestBodies) ?? new List<NodeBody>());
            result.AddRange(await GetChatNodesByIdAsync(accountId, requestBodies) ?? new List<NodeBody>());

            foreach (var node in result)
            {
                var matchingRequest = requestBodies.FirstOrDefault(r => r.Id == node.Id);
                if (matchingRequest != null)
                {
                    node.SyncKind = matchingRequest.SyncKind;
                    node.Version = matchingRequest.Version;
                    node.ScopeVersion = matchingRequest.ScopeVersion;
                }
            }

            return result;
        }

        // Загрузка нод, сохранение их в БД и отправка событий в другие сервисы для синхронизации данных
        public async Task<ServiceResponse<List<NodeBody>>> LoadNodes(List<NodeBody> nodeBodies, TokenPayload tokenPayload)
        {
            var newNodes = await _nodeRepository.AddOrUpdateNodes(nodeBodies);

            List<ISyncable> contentBodies = new List<ISyncable>();
            List<ISyncable> chatBodies = new List<ISyncable>();

            contentBodies.AddRange(nodeBodies.OfType<BoardBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            contentBodies.AddRange(nodeBodies.OfType<ColumnBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            contentBodies.AddRange(nodeBodies.OfType<JobBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());

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

        // Получение полных тел нод из сервиса контента по списку Id нод
        public async Task<List<NodeBody>> GetContentNodesByIdAsync(List<NodeBody> nodes)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://planner-content-service:8080/api/"),
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

        // Получение полных тел нод из сервиса чата по списку Id нод
        public async Task<List<NodeBody>> GetChatNodesByIdAsync(Guid accountId, List<NodeBody> nodes)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://planner-chat-service:8080/api/"),
            };

            var nodeIds = nodes.Select(x => x.Id);

            var queryString = string.Join("&", nodeIds.Select(id => $"nodeIds={id}&accountId={accountId}"));

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

        public async Task<List<NodeBody>> FillNodes(Guid accountId, List<NodeBody> nodes)
        {
            foreach (var body in nodes)
            {
                // Получаем ноду из репозитория, чтобы узнать ее версию, и добавляем эту информацию в тело ноды
                var node = await _nodeRepository.GetNode(body.Id);

                if (node != null)
                {
                    body.Version = node.Version;
                }
                else
                {
                    continue;
                }

                // Получаем историю изменений ноды, чтобы узнать, кто и когда в последний раз изменял ноду, и добавляем эту информацию в тело ноды
                var history = await _logRepository.GetLastHistory(body.Id);

                if (history != null)
                {
                    body.UpdatedBy = history.UpdatedById;
                    body.UpdatedAt = history.UpdatedAt;
                }

                // Если нода является Scope, получаем версию Scope из последнего лога для Scope и добавляем эту информацию в тело ноды
                if (body.SyncKind == SyncKind.Scope)
                {
                    var scopeLog = await _logRepository.GetLastLogForScope(body.Id);

                    if (scopeLog != null)
                        body.ScopeVersion = scopeLog.ScopeVersion;
                }

                // Получаем информацию о родителе ноды и правила доступа к ноде, и добавляем эту информацию в тело ноды
                var link = await _nodeRepository.GetNodeLink(body.Id);
                var access = await _accessRepository.GetAccessRuleForNode(accountId, body.Id);

                if (link != null)
                    body.Link = link.ToBody();

                if (access != null)
                    body.AccessRule = access.ToBody();
            }

            return nodes;
        }
    }
}

