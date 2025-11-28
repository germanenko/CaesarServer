using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.IService
{
    public interface IAccessService
    {
        public Task<ServiceResponse<NodeBody>> CreateOrUpdateAccessGroup(Guid accountId, CreateAccessGroupBody body);
        public Task<ServiceResponse<NodeBody>> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId);
        public Task<ServiceResponse<NodeBody>> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
    }
}
