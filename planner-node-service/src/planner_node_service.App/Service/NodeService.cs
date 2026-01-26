using Microsoft.IdentityModel.Tokens;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.Interface;
using System.Text.Json;

namespace planner_node_service.App.Service
{
    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly INotifyService _notifyService;

        public NodeService(
            INodeRepository nodeRepository, INotifyService notifyService)
        {
            _nodeRepository = nodeRepository;
            _notifyService = notifyService;
        }

        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodes(Guid accountId)
        {
            var nodes = await _nodeRepository.GetNodes(accountId);

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodes.Select(x => x.ToNodeBodyFromJson())
            };
        }

        public async Task<ServiceResponse<IEnumerable<NodeLink>>> GetNodeLinks(Guid accountId)
        {
            var nodeLinks = await _nodeRepository.GetNodeLinks(accountId);

            return new ServiceResponse<IEnumerable<NodeLink>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeLinks?
                  .Where(x => x != null)
                  .ToList()!
               ?? new List<NodeLink>()
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

            contentBodies.AddRange(bodies.OfType<BoardBody>().ToList());
            contentBodies.AddRange(bodies.OfType<ColumnBody>().ToList());
            contentBodies.AddRange(bodies.OfType<TaskBody>().ToList());

            chatBodies.AddRange(bodies.OfType<ChatBody>().ToList());
            chatBodies.AddRange(bodies.OfType<MessageBody>().ToList());
            chatBodies.AddRange(bodies.OfType<ChatSettingsBody>().ToList());

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
    }
}

