using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Core.IRepository
{
    public interface IAccessRepository
    {

        Task<AccessRight?> CreateAccessRight(Guid accountId, Guid nodeId, AccessType accessType);
        Task<AccessRight?> CreateAccessRight(AccessRightBody accessRightBody);
        Task<AccessGroup?> CreateGroup(Guid accountId, CreateAccessGroupBody body);
        Task<AccessGroupMember?> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId);
        Task<AccessGroupMember?> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
        Task<AccessBody?> GetAccessRights(Guid accountId);
        Task<bool> CheckAccess(Guid accountId, Guid nodeId);
    }
}