using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface  INodeRepository
    {
        Task<NodeLink> AddOrUpdateNodeLink(Guid accountId, CreateOrUpdateNodeLink node);
        Task<Node> AddOrUpdateNode(Guid accountId, Node node);
        Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null);
        Task<IEnumerable<NodeLink>?> GetNodeLinks(Guid accountId);
        Task<IEnumerable<Node>?> GetNodes(Guid accountId);
    }
}