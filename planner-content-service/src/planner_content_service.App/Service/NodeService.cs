using Microsoft.IdentityModel.Tokens;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_server_package;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Net;

namespace planner_content_service.App.Service
{
    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IPublisherService _publisherService;

        public NodeService(
            INodeRepository nodeRepository,
            IPublisherService publisherService)
        {
            _nodeRepository = nodeRepository;
            _publisherService = publisherService;
        }

        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodesByIds(List<Guid> nodeIds)
        {
            var nodes = await _nodeRepository.GetNodesByIds(nodeIds);

            return new ServiceResponse<IEnumerable<NodeBody>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodes?.Select(x => x.ToNodeBody())
            };
        }

        public async Task<ServiceResponse<IEnumerable<Node>>> GetNodes(Guid accountId, List<Guid> rootIds)
        {
            var nodes = await _nodeRepository.GetNodes(accountId, rootIds);

            return new ServiceResponse<IEnumerable<Node>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodes
            };
        }

        public async Task<ServiceResponse<NodeBody>> AddOrUpdateNode(Guid accountId, Node node)
        {
            var newNode = await _nodeRepository.AddOrUpdateNode(accountId, node);

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = newNode.ToNodeBody()
            };
        }

        public async Task<ServiceResponse<bool>> DeleteNode(Guid accountId, Guid columnId, CancellationToken cancellationToken = default)
        {
            DeleteNodeEvent deleteEvent = new DeleteNodeEvent()
            {
                NodeId = columnId,
                AccountId = accountId
            };

            var request = await _publisherService.Publish(deleteEvent, PublishEvent.DeleteNode);

            if (!request.IsSuccess)
            {
                return new ServiceResponse<bool>
                {
                    IsSuccess = request.IsSuccess,
                    StatusCode = request.StatusCode,
                    Errors = request.Errors
                };
            }

            var result = await _nodeRepository.DeleteNode(columnId, accountId, cancellationToken);

            if (!result)
            {
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Нода не удалена" },
                    ErrorCodes = [ErrorCode.Infrastructure]
                };
            }

            return new ServiceResponse<bool>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }
    }
}
