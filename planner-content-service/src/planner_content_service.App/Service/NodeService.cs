using Microsoft.IdentityModel.Tokens;
using planner_client_package.Entities;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using System.Collections.Generic;

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
    }
}
