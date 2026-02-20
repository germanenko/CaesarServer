using planner_chat_service.Core.Entities.Models;
using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_chat_service.Core.IRepository
{
    public interface INodeRepository
    {
        Task<Node> AddOrUpdateNode(Guid accountId, Node node);
        Task<IEnumerable<Node>?> GetNodes(Guid accountId, List<Guid> nodeIds);
        Task<IEnumerable<Node>?> GetNodesByIds(List<Guid> nodeIds);
    }
}