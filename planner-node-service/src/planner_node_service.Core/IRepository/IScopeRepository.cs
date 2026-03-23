using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.IRepository
{
    public interface IScopeRepository
    {
        Task<Node?> GetNodeScope(Guid nodeId);
        Task<AccessRule?> CheckScopeAccess(Guid accountId, Guid scopeId);
        Task<IEnumerable<Node>?> GetScopes(Guid accountId);
        Task<SyncScopeAccess?> GetSyncScopeAccess(Guid accountId, Guid scopeId);
    }
}
