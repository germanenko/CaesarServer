using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface IAccessRepository
    {
        Task<Node?> CreateOrUpdateGroup(Guid accountId, CreateAccessGroupBody body);
        Task<Node?> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId);
        Task<Node?> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
        Task<bool> CheckAccess(Guid accountId, Guid nodeId);
    }
}