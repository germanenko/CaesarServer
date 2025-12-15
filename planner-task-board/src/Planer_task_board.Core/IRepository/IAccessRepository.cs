using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface IAccessRepository
    {
        Task<AccessGroup?> CreateGroup(Guid accountId, CreateAccessGroupBody body);
        Task<AccessGroupMember?> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId);
        Task<AccessGroupMember?> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
        Task<List<AccessRight>?> GetAccessRights(Guid accountId);
        Task<bool> CheckAccess(Guid accountId, Guid nodeId);
    }
}