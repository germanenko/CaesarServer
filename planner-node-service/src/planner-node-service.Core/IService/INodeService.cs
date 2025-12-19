using FirebaseAdmin.Messaging;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.Entities.Request;
using planner_node_service.Core.Entities.Response;

namespace planner_node_service.Core.IService
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
