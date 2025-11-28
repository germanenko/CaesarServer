using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Planer_task_board.App.Service
{
    public class AccessService : IAccessService
    {
        private readonly IAccessRepository _accessRepository;

        public AccessService(IAccessRepository accessRepository)
        {
            _accessRepository = accessRepository;
        }

        public async Task<ServiceResponse<NodeBody>> CreateOrUpdateAccessGroup(Guid accountId, CreateAccessGroupBody body)
        {
            var group = await _accessRepository.CreateOrUpdateGroup(accountId, body);

            if(group == null)
            {
                return new ServiceResponse<NodeBody>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new [] { "Нет доступа к доске" } 
                };
            }

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = group.ToNodeBody()
            };
        }

        public async Task<ServiceResponse<NodeBody>> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId)
        {
            var group = await _accessRepository.AddUserToGroup(accountId, userToAdd, groupId);

            if(group == null)
            {
                return new ServiceResponse<NodeBody>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = group.ToNodeBody()
            };
        }

        public async Task<ServiceResponse<NodeBody>> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId)
        {
            var group = await _accessRepository.RemoveUserFromGroup(accountId, userToRemove, groupId);

            if (group == null)
            {
                return new ServiceResponse<NodeBody>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            return new ServiceResponse<NodeBody>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = group.ToNodeBody()
            };
        }
    }
}
