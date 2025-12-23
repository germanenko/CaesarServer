using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using planner_node_service.Core.Entities.Models;
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
        Task<NodeLink> AddOrUpdateNodeLink(CreateOrUpdateNodeLink node);
        Task<List<NodeLink>> AddOrUpdateNodeLinks(List<CreateOrUpdateNodeLink> nodes);
        Task<Node> AddOrUpdateNode(Node node);
        Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null);
        Task<IEnumerable<NodeLink>?> GetNodeLinks(Guid accountId);
        Task<IEnumerable<Node>?> GetNodes(Guid accountId);
    }
}
