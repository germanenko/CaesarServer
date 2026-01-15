using CaesarServerLibrary.Entities;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using System.Net;

namespace planner_content_service.App.Service
{
    public class AccessService : IAccessService
    {
        private readonly IAccessRepository _accessRepository;
        private readonly IUserService _userService;

        public AccessService(IAccessRepository accessRepository, IUserService userService)
        {
            _accessRepository = accessRepository;
            _userService = userService;
        }

        public async Task<ServiceResponse<AccessGroupBody>> CreateAccessGroup(Guid accountId, CreateAccessGroupBody body)
        {
            var group = await _accessRepository.CreateGroup(accountId, body);

            if(group == null)
            {
                return new ServiceResponse<AccessGroupBody>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new [] { "Нет доступа к доске" } 
                };
            }

            return new ServiceResponse<AccessGroupBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = group.ToAccessGroupBody()
            };
        }

        public async Task<ServiceResponse<AccessGroupMemberBody>> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId)
        {
            var group = await _accessRepository.AddUserToGroup(accountId, userToAdd, groupId);

            if(group == null)
            {
                return new ServiceResponse<AccessGroupMemberBody>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            return new ServiceResponse<AccessGroupMemberBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = group.ToAccessGroupMemberBody()
            };
        }

        public async Task<ServiceResponse<HttpStatusCode>> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId)
        {
            var group = await _accessRepository.RemoveUserFromGroup(accountId, userToRemove, groupId);

            if (group == null)
            {
                return new ServiceResponse<HttpStatusCode>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            return new ServiceResponse<HttpStatusCode>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = HttpStatusCode.OK
            };
        }

        public async Task<ServiceResponse<AccessBody>> GetAccessRights(Guid accountId)
        {
            var access = await _accessRepository.GetAccessRights(accountId);

            if (access == null)
            {
                return new ServiceResponse<AccessBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Body = new AccessBody()
                };
            }

            var accountIds = new List<Guid>();

            if (access.AccessRights != null)
            {
                accountIds.AddRange(access.AccessRights
                    .Where(x => x.AccountId.HasValue)
                    .Select(x => x.AccountId.Value));
            }

            if (access.AccessGroupMembers != null)
            {
                accountIds.AddRange(access.AccessGroupMembers
                    .Select(x => x.AccountId));
            }

            var profileTasks = accountIds.Distinct()
            .Select(id => _userService.GetUserData(id))
            .ToList();

            var profiles = await Task.WhenAll(profileTasks);

            access.Profiles = profiles
                .Where(profile => profile != null)
                .Distinct()
                .ToList();

            return new ServiceResponse<AccessBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access
            };
        }
    }
}
