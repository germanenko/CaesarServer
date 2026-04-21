using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Core.IRepository
{
    public interface IAccessRepository
    {
        Task<AccessRule?> AddAccess(Guid accountId, Guid nodeId, Permission permission);
        Task<AccessRule?> ChangePermission(Guid granterId, Guid granteeId, Guid nodeId, Permission permission);
        Task<bool> RevokeAccess(Guid granterId, Guid granteeId, Guid nodeId);
        Task<GroupAccessSubject?> CreateGroup(Guid accountId, CreateAccessGroupBody body);
        Task<GroupMember?> AddUserToGroup(Guid granterId, Guid granteeId, Guid groupId);
        Task<GroupMember?> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
        Task<List<AccessRuleBody>?> GetAccessRules(Guid accountId);
        Task<List<AccessRuleBody>?> GetCommonAccessRules(Guid accountId);
        Task<AccessRule?> GetAccessRuleForNode(Guid accountId, Guid nodeId);
        Task<bool> CheckAccess(Guid accountId, Guid nodeId, Permission minRequiredPermission);
    }
}