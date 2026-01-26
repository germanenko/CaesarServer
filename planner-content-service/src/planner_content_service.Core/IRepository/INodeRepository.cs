using planner_server_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface INodeRepository
    {
        Task<NodeLink> AddOrUpdateNodeLink(Guid accountId, NodeLinkBody node);
        Task<List<NodeLink>> AddOrUpdateNodeLinks(Guid accountId, List<NodeLinkBody> nodes);
        Task<Node> AddOrUpdateNode(Guid accountId, Node node);
        Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null);
        Task<IEnumerable<NodeLink>?> GetNodeLinks(Guid accountId);
        Task<IEnumerable<Node>?> GetNodes(Guid accountId);
    }
}