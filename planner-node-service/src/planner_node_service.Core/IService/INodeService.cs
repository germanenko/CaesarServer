using planner_client_package.Entities;
using planner_common_package.Entities;
using planner_node_service.Core.Entities.Models;
using planner_server_package;

namespace planner_node_service.Core.IService
{
    public interface INodeService
    {
        public Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodes(Guid accountId, List<Guid>? rootIds = null);
        public Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodesByIds(Guid accountId, List<Guid> nodeIds);
        public Task<ServiceResponse<List<EntityVersionBody>>> GetManifests(Guid accountId, List<Guid> scopeIds);
        public Task<ServiceResponse<List<EntityVersionBody>>> GetScopesManifest(Guid accountId);
        public Task<ServiceResponse<NodeBody>> AddOrUpdateScope(NodeBody node);
        public Task<ServiceResponse<NodeBody>> AddOrUpdateNode(NodeBody node);
        public Task<ServiceResponse<bool>> DeleteNode(Guid accountId, Guid nodeId);
        public Task<ServiceResponse<NodeLinkBody>> ChangeNodeParent(Guid accountId, Guid nodeId, Guid newParentId);
        public Task<ServiceResponse<List<NodeBody>>> LoadNodes(List<NodeBody> nodes, TokenPayload tokenPayload);
    }
}
