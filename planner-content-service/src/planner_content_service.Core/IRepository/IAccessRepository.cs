using CaesarServerLibrary.Entities;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface IAccessRepository
    {
        Task<AccessGroup?> CreateGroup(Guid accountId, CreateAccessGroupBody body);
        Task<AccessGroupMember?> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId);
        Task<AccessGroupMember?> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
        Task<AccessBody?> GetAccessRights(Guid accountId);
        Task<bool> CheckAccess(Guid accountId, Guid nodeId);
    }
}