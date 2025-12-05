using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Planer_task_board.App.Service
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
                Body = nodes?.Select(x => x.ToNodeBody())
                  .Where(x => x != null)
                  .ToList()!
               ?? new List<NodeBody>()
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

        public async Task<ServiceResponse<NodeLink>> AddOrUpdateNodeLink(Guid accountId, CreateOrUpdateNodeLink node)
        {
            var nodeLink = await _nodeRepository.AddOrUpdateNodeLink(accountId, node);

            return new ServiceResponse<NodeLink>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodeLink
            };
        }
    }
}
