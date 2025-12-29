using CaesarServerLibrary.Entities;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;

namespace planner_node_service.App.Service
{
    public class NodeService : INodeService
    {
        private readonly INodeRepository _nodeRepository;

        public NodeService(
            INodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
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
    }
}

