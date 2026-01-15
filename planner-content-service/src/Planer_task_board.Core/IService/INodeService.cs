using CaesarServerLibrary.Entities;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.IService
{
    public interface INodeService
    {
        public Task<ServiceResponse<IEnumerable<Node>>> GetNodes(Guid accountId);
        public Task<ServiceResponse<IEnumerable<NodeLink>>> GetNodeLinks(Guid accountId);
        public Task<ServiceResponse<NodeBody>> AddOrUpdateNode(Guid accountId, Node node);
        public Task<ServiceResponse<NodeLink>> AddOrUpdateNodeLink(Guid accountId, NodeLinkBody node);
        public Task<ServiceResponse<List<NodeLink>>> AddOrUpdateNodeLinks(Guid accountId, List<NodeLinkBody> nodes);
    }
}
