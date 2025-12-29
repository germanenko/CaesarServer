using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.IRepository
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