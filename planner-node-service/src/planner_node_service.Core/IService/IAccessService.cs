using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_server_package;
using System.Net;

namespace planner_node_service.Core.IService
{
    public interface IAccessService
    {
        public Task<ServiceResponse<AccessRuleBody>> AddAccess(Guid accountId, Guid nodeId, Permission permission);
        public Task<ServiceResponse<AccessRuleBody>> GrantAccess(Guid granterId, Guid granteeId, Guid nodeId, Permission permission);
        public Task<ServiceResponse<AccessRuleBody>> ChangePermission(Guid granterId, Guid granteeId, Guid nodeId, Permission permission);
        public Task<ServiceResponse<bool>> RevokeAccess(Guid granterId, Guid granteeId, Guid nodeId);
        public Task<ServiceResponse<AccessGroupBody>> CreateAccessGroup(Guid accountId, CreateAccessGroupBody body);
        public Task<ServiceResponse<AccessGroupMemberBody>> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId);
        public Task<ServiceResponse<HttpStatusCode>> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
        public Task<ServiceResponse<AccessBody>> GetAccessRules(Guid accountId);
        public Task<ServiceResponse<bool>> CheckAccess(Guid accountId, Guid nodeId, Permission minRequiredPermission);
    }
}
