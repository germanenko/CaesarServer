using Microsoft.IdentityModel.Tokens;
using planner_client_package.Entities;
using planner_common_package.Entities;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.Interface;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace planner_node_service.App.Service
{
    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly INotifyService _notifyService;

        public NodeService(
            INodeRepository nodeRepository, INotifyService notifyService, IHistoryRepository historyRepository)
        {
            _nodeRepository = nodeRepository;
            _notifyService = notifyService;
            _historyRepository = historyRepository;
        }

        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodes(Guid accountId)
        {
            var nodes = await _nodeRepository.GetNodes(accountId);

            var bodies = nodes.Select(x => x.ToNodeBody()).ToList();

            bodies = await GetNodesByIdAsync(bodies);

            foreach (var body in bodies)
            {
                var history = await _historyRepository.GetLastHistory(body.Id);
                body.UpdatedBy = history?.ActorId;
                body.UpdatedAt = history?.Date;
            }

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = bodies
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

        public async Task<ServiceResponse<NodeBody>> AddOrUpdateNode(Node node)
        {
            if (node.Name.IsNullOrEmpty())
            {
                return new ServiceResponse<NodeBody>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Errors = new[] { "Name field is reqiured" }
                };
            }

            var newNode = await _nodeRepository.AddOrUpdateNode(node);

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNode.ToNodeBody()
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

        public async Task<ServiceResponse<List<NodeBody>>> LoadNodes(List<NodeBody> nodeBodies, TokenPayload tokenPayload)
        {
            List<Node> nodes = new List<Node>();
            foreach (NodeBody nodeBody in nodeBodies)
            {
                nodes.Add(new Node()
                {
                    Id = nodeBody.Id,
                    Name = nodeBody.Name,
                    Type = nodeBody.Type,
                    BodyJson = JsonSerializer.Serialize(nodeBody)
                });
            }

            var newNodes = await _nodeRepository.AddOrUpdateNodes(nodes);

            var bodies = newNodes.Select(x => x.ToNodeBodyFromJson()).ToList();
            List<ISyncable> contentBodies = new List<ISyncable>();
            List<ISyncable> chatBodies = new List<ISyncable>();

            contentBodies.AddRange(bodies.OfType<BoardBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            contentBodies.AddRange(bodies.OfType<ColumnBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            contentBodies.AddRange(bodies.OfType<TaskBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());

            chatBodies.AddRange(bodies.OfType<ChatBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            chatBodies.AddRange(bodies.OfType<MessageBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());
            chatBodies.AddRange(bodies.OfType<ChatSettingsBody>().Select(x => BodyConverter.ClientToServerBody(x)).ToList());

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

            _notifyService.Publish(contentNodesEvent, PublishEvent.ContentNodes);
            _notifyService.Publish(chatNodesEvent, PublishEvent.ChatNodes);

            return new ServiceResponse<List<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNodes.Select(x => x.ToNodeBodyFromJson()).ToList()
            };
        }

        public async Task<List<NodeBody>> GetNodesByIdAsync(List<NodeBody> nodes)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://planner-content-service:80/api/"),
            };

            var s = JsonSerializer.Serialize(nodes.Select(x => x.Id));
            var content = new StringContent(s, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await client.PostAsync("getNodesByIds", content);


            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var result = JsonSerializer.Deserialize<List<NodeBody>>(resultString, options);

                return result;
            }
            else
            {
                return nodes;
            }
        }
    }
}

