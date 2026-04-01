using planner_client_package.Entities;
using planner_content_service.Core.Entities.Models;
using planner_server_package;

namespace planner_content_service.Core.IService
{
    public interface INodeService
    {
        public Task<ServiceResponse<IEnumerable<NodeBody>>> GetNodesByIds(List<Guid> nodeIds);
        public Task<ServiceResponse<IEnumerable<Node>>> GetNodes(Guid accountId, List<Guid> rootIds);
        public Task<ServiceResponse<NodeBody>> AddOrUpdateNode(Guid accountId, Node node);
    }
}
