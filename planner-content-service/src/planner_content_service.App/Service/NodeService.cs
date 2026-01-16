using planner_server_package.Entities;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;

namespace planner_content_service.App.Service
{
    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;

        public NodeService(
            INodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Node>>> GetNodes(Guid accountId)
        {
            var nodes = await _nodeRepository.GetNodes(accountId);

            return new ServiceResponse<IEnumerable<Node>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodes
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

        public async Task<ServiceResponse<NodeLink>> AddOrUpdateNodeLink(Guid accountId, NodeLinkBody node)
        {
            var nodeLink = await _nodeRepository.AddOrUpdateNodeLink(accountId, node);

            return new ServiceResponse<NodeLink>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeLink
            };
        }

        public async Task<ServiceResponse<List<NodeLink>>> AddOrUpdateNodeLinks(Guid accountId, List<NodeLinkBody> nodes)
        {
            var nodeLink = await _nodeRepository.AddOrUpdateNodeLinks(accountId, nodes);

            return new ServiceResponse<List<NodeLink>>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeLink
            };
        }
    }
}
