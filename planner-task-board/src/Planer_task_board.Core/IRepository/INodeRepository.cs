using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface INodeRepository
    {
        Task<Node> AddOrUpdateNode(Guid accountId, Node node);
        Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null);
        Task<IEnumerable<Node>?> GetNodes(Guid accountId);
    }
}