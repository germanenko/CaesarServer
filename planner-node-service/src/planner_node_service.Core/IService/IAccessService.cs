using planner_client_package.Entities;
using planner_common_package.Enums;
using System.Net;

namespace planner_node_service.Core.IService
{
    public interface IAccessService
    {
        public Task<ServiceResponse<AccessRightBody>> CreateAccessRight(Guid accountId, Guid nodeId, AccessType accessType);
        public Task<ServiceResponse<AccessRightBody>> CreateAccessRight(AccessRightBody accessRightBody);
        public Task<ServiceResponse<AccessGroupBody>> CreateAccessGroup(Guid accountId, CreateAccessGroupBody body);
        public Task<ServiceResponse<AccessGroupMemberBody>> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId);
        public Task<ServiceResponse<HttpStatusCode>> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
        public Task<ServiceResponse<AccessBody>> GetAccessRights(Guid accountId);
        public Task<ServiceResponse<bool>> CheckAccess(Guid accountId, Guid nodeId);
    }
}
