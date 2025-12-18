using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Planer_task_board.App.Service
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
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступов" }
                };
            }

            var profiles = new List<ProfileBody>();

            access.AccessRights.Select(async x => profiles.Add(await _userService.GetUserData(x.AccountId.Value)));
            access.AccessGroupMembers.Select(async x => profiles.Add(await _userService.GetUserData(x.AccountId)));

            access.Profiles = profiles.Distinct().ToList();

            return new ServiceResponse<AccessBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access
            };
        }
    }
}
