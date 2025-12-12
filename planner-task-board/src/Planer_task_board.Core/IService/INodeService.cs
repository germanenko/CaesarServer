using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.IService
{
    public interface INodeService
    {
        public Task<ServiceResponse<IEnumerable<Node>>> GetNodes(Guid accountId);
        public Task<ServiceResponse<IEnumerable<NodeLink>>> GetNodeLinks(Guid accountId);
        public Task<ServiceResponse<NodeBody>> AddOrUpdateNode(Guid accountId, Node node);
        public Task<ServiceResponse<NodeLink>> AddOrUpdateNodeLink(Guid accountId, CreateOrUpdateNodeLink node);
        public Task<ServiceResponse<List<NodeLink>>> AddOrUpdateNodeLinks(Guid accountId, List<CreateOrUpdateNodeLink> nodes);
    }
}
