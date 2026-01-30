using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Core.IRepository
{
    public interface INodeRepository
    {
        Task<NodeLink> AddOrUpdateNodeLink(NodeLinkBody node);
        Task<List<Node>> AddOrUpdateNodes(List<Node> node);
        Task<List<NodeLink>> AddOrUpdateNodeLinks(List<NodeLinkBody> nodes);
        Task<Node> AddOrUpdateNode(Node node);
        Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null);
        Task<IEnumerable<Node>?> GetNodesTree(Guid accountId);
        Task<IEnumerable<NodeLink>?> GetNodesLinks(Guid accountId);
        Task<IEnumerable<Node>?> GetNodes(Guid accountId);
    }
}
