using planner_client_package.Entities;
using planner_common_package.Enums;
using System;
using System.Threading.Tasks;

namespace planner_server_package.Access
{
    public interface IAccessService
    {
        Task<bool> CheckAccess(Guid accountId, Guid nodeId, Permission minRequiredPermission);
    }
}
