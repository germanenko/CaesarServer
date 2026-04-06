using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using planner_server_package;
using System.Net;

namespace planner_node_service.App.Service
{
    public class AccessService : IAccessService
    {
        private readonly IAccessRepository _accessRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly IUserService _userService;

        public AccessService(IAccessRepository accessRepository, INodeRepository nodeRepository, IUserService userService)
        {
            _accessRepository = accessRepository;
            _nodeRepository = nodeRepository;
            _userService = userService;
        }

        public async Task<ServiceResponse<AccessRuleBody>> GrantAccess(Guid granterId, Guid granteeId, Guid nodeId, Permission permission)
        {
            if (_nodeRepository.GetNode(nodeId) == null)
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ноды не существует" }
                };
            }

            if (!await _accessRepository.CheckAccess(granterId, nodeId, Permission.Write))
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Отказано в выдаче доступа" }
                };
            }

            var access = await _accessRepository.GrantAccess(granterId, granteeId, nodeId, permission);

            if (access == null)
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Доступ не выдан" }
                };
            }

            return new ServiceResponse<AccessRuleBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access.ToBody()
            };
        }

        public async Task<ServiceResponse<AccessRuleBody>> ChangePermission(Guid granterId, Guid granteeId, Guid nodeId, Permission permission)
        {
            if (_nodeRepository.GetNode(nodeId) == null)
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ноды не существует" }
                };
            }

            if (!await _accessRepository.CheckAccess(granterId, nodeId, Permission.Write))
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Отказано в изменении разрешения" }
                };
            }

            var access = await _accessRepository.ChangePermission(granterId, granteeId, nodeId, permission);

            if (access == null)
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Уровень доступа не изменен" }
                };
            }

            return new ServiceResponse<AccessRuleBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access.ToBody()
            };
        }

        public async Task<ServiceResponse<bool>> RevokeAccess(Guid granterId, Guid granteeId, Guid nodeId)
        {
            if (_nodeRepository.GetNode(nodeId) == null)
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ноды не существует" }
                };
            }

            if (!await _accessRepository.CheckAccess(granterId, nodeId, Permission.Write))
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Отказано в отзыве доступа" }
                };
            }

            var access = await _accessRepository.RevokeAccess(granterId, granteeId, nodeId);

            if (access == false)
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Доступ не отозван" }
                };
            }

            return new ServiceResponse<bool>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access
            };
        }

        public async Task<ServiceResponse<AccessRuleBody>> CreateAccessRule(Guid accountId, Guid nodeId, Permission permission)
        {
            var access = await _accessRepository.GrantAccess(accountId, accountId, nodeId, permission);

            if (access == null)
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа к доске" }
                };
            }

            return new ServiceResponse<AccessRuleBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access.ToBody()
            };
        }

        public async Task<ServiceResponse<AccessGroupBody>> CreateAccessGroup(Guid accountId, CreateAccessGroupBody body)
        {
            var group = await _accessRepository.CreateGroup(accountId, body);

            if (group == null)
            {
                return new ServiceResponse<AccessGroupBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа к доске" }
                };
            }

            return new ServiceResponse<AccessGroupBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = group.ToAccessGroupBody()
            };
        }

        public async Task<ServiceResponse<AccessGroupMemberBody>> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId)
        {
            var group = await _accessRepository.AddUserToGroup(accountId, userToAdd, groupId);

            if (group == null)
            {
                return new ServiceResponse<AccessGroupMemberBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            return new ServiceResponse<AccessGroupMemberBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
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
                    StatusCode = HttpStatusCode.Forbidden,
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

        public async Task<ServiceResponse<AccessBody>> GetAccessRules(Guid accountId)
        {
            var access = await _accessRepository.GetAccessRules(accountId);

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

            if (access.AccessRules != null)
            {
                accountIds.AddRange(access.AccessRules
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

        public async Task<ServiceResponse<bool>> CheckAccess(Guid accountId, Guid nodeId, Permission requiredPermission)
        {
            var access = await _accessRepository.CheckAccess(accountId, nodeId, requiredPermission);

            if (access == true)
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = true,
                    Body = access,
                    StatusCode = HttpStatusCode.OK
                };
            }
            else
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = true,
                    Body = access,
                    StatusCode = HttpStatusCode.Forbidden
                };
            }
        }
    }
}
