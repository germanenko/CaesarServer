using planner_server_package.Entities;
using System.Net;

namespace planner_content_service.Core.IService
{
    public interface IAccessService
    {
        public Task<ServiceResponse<AccessGroupBody>> CreateAccessGroup(Guid accountId, CreateAccessGroupBody body);
        public Task<ServiceResponse<AccessGroupMemberBody>> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId);
        public Task<ServiceResponse<HttpStatusCode>> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId);
        public Task<ServiceResponse<AccessBody>> GetAccessRights(Guid accountId);
    }
}
