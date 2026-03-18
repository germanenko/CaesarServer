using planner_client_package.Entities;
using planner_common_package.Entities;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Core.IService
{
    public interface INodeService
    {
        public Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodes(Guid accountId);
        public Task<ServiceResponse<IEnumerable<NodeBody>>> GetScopes(Guid accountId);
        public Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodesByIds(Guid accountId, List<Guid> nodeIds);
        public Task<ServiceResponse<List<EntityVersionBody>>> GetManifest(Guid accountId);
        public Task<ServiceResponse<List<EntityVersionBody>>> GetScopesManifest(Guid accountId);
        public Task<ServiceResponse<IEnumerable<NodeLinkBody>>> GetNodeLinks(Guid accountId);
        public Task<ServiceResponse<NodeBody>> AddScope(NodeBody node);
        public Task<ServiceResponse<NodeBody>> AddOrUpdateNode(NodeBody node);
        public Task<ServiceResponse<NodeLink>> AddOrUpdateNodeLink(NodeLinkBody node);
        public Task<ServiceResponse<List<NodeLink>>> AddOrUpdateNodeLinks(List<NodeLinkBody> nodes);
        public Task<ServiceResponse<List<NodeBody>>> LoadNodes(List<NodeBody> nodes, TokenPayload tokenPayload);
    }
}
