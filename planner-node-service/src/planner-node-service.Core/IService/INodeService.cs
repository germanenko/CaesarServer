using CaesarServerLibrary.Entities;
using FirebaseAdmin.Messaging;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Core.IService
{
    public interface INodeService
    {
        public Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodes(Guid accountId);
        public Task<ServiceResponse<IEnumerable<NodeLink>>> GetNodeLinks(Guid accountId);
        public Task<ServiceResponse<NodeBody>> AddOrUpdateNode(Node node);
        public Task<ServiceResponse<NodeLink>> AddOrUpdateNodeLink(CreateOrUpdateNodeLink node);
        public Task<ServiceResponse<List<NodeLink>>> AddOrUpdateNodeLinks(List<CreateOrUpdateNodeLink> nodes);
    }
}
