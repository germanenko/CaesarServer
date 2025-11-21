using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<ServiceResponse<Node>> AddOrUpdateNode(Guid accountId, Node node)
        {
            var nodes = await _nodeRepository.AddOrUpdateNode(accountId, node);

            return new ServiceResponse<Node>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = nodes
            };
        }
    }
}
