using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.Entities.Request;
using planner_node_service.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.IRepository
{
    public interface INodeRepository
    {
        Task<NodeLink> AddOrUpdateNodeLink(Guid accountId, CreateOrUpdateNodeLink node);
        Task<List<NodeLink>> AddOrUpdateNodeLinks(Guid accountId, List<CreateOrUpdateNodeLink> nodes);
        Task<Node> AddOrUpdateNode(Guid accountId, Node node);
        Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null);
        Task<IEnumerable<NodeLink>?> GetNodeLinks(Guid accountId);
        Task<IEnumerable<Node>?> GetNodes(Guid accountId);
    }
}
