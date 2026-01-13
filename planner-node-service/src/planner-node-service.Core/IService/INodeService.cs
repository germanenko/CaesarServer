using CaesarServerLibrary.Entities;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Core.IService
{
    public interface INodeService
    {
        public Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodes(Guid accountId);
        public Task<ServiceResponse<IEnumerable<NodeLink>>> GetNodeLinks(Guid accountId);
        public Task<ServiceResponse<NodeBody>> AddOrUpdateNode(Node node);
        public Task<ServiceResponse<NodeLink>> AddOrUpdateNodeLink(NodeLinkBody node);
        public Task<ServiceResponse<List<NodeLink>>> AddOrUpdateNodeLinks(List<NodeLinkBody> nodes);
        public Task<ServiceResponse<List<NodeBody>>> LoadNodes(List<NodeBody> nodes);
    }
}
