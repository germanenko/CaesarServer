using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using planner_server_package;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Net;

namespace planner_node_service.App.Service
{
    public class AccessService : IAccessService
    {
        private readonly IAccessRepository _accessRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly IPublisherService _publisherService;

        public AccessService(IAccessRepository accessRepository, INodeRepository nodeRepository, IPublisherService publisherService)
        {
            _accessRepository = accessRepository;
            _nodeRepository = nodeRepository;
            _publisherService = publisherService;
        }

        // Создание доступа
        public async Task<ServiceResponse<AccessRuleBody>> AddAccess(Guid accountId, Guid nodeId, Permission permission)
        {
            // Проверяем, существует ли нода
            if (await _nodeRepository.GetNode(nodeId) == null)
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ноды не существует" }
                };
            }

            // Выдаем доступ
            var access = await _accessRepository.AddAccess(accountId, nodeId, permission);

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

        // Выдача доступа
        public async Task<ServiceResponse<AccessRuleBody>> GrantAccess(Guid granterId, Guid granteeId, Guid nodeId, Permission permission)
        {
            // Проверяем, существует ли нода
            if (await _nodeRepository.GetNode(nodeId) == null)
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ноды не существует" }
                };
            }

            // Проверяем, имеет ли грантер право выдавать доступ к этой ноде
            if (!await _accessRepository.CheckAccess(granterId, nodeId, Permission.Write))
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Отказано в выдаче доступа" }
                };
            }

            // Выдаем доступ
            var access = await _accessRepository.AddAccess(granteeId, nodeId, permission);

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

        // Изменение уровня доступа
        public async Task<ServiceResponse<AccessRuleBody>> ChangePermission(Guid granterId, Guid granteeId, Guid nodeId, Permission permission)
        {
            // Проверяем, существует ли нода
            if (await _nodeRepository.GetNode(nodeId) == null)
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ноды не существует" }
                };
            }

            // Проверяем, имеет ли грантер право изменять доступ к этой ноде
            if (!await _accessRepository.CheckAccess(granterId, nodeId, Permission.Write))
            {
                return new ServiceResponse<AccessRuleBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Отказано в изменении разрешения" }
                };
            }

            // Изменяем уровень доступа
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

        // Отзыв доступа
        public async Task<ServiceResponse<bool>> RevokeAccess(Guid granterId, Guid granteeId, Guid nodeId)
        {
            // Проверяем, существует ли нода
            if (await _nodeRepository.GetNode(nodeId) == null)
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ноды не существует" }
                };
            }

            // Проверяем, имеет ли грантер право отзывать доступ к этой ноде
            if (!await _accessRepository.CheckAccess(granterId, nodeId, Permission.Write))
            {
                return new ServiceResponse<bool>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Отказано в отзыве доступа" }
                };
            }

            // Отзываем доступ
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

            var accessRevokedEvent = new AccessRevokedEvent()
            {
                AccountId = granteeId,
                NodeId = nodeId
            };

            await _publisherService.Publish(accessRevokedEvent, PublishEvent.AccessRevoked);

            return new ServiceResponse<bool>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access
            };
        }

        // Создание группы доступа
        public async Task<ServiceResponse<AccessGroupBody>> CreateAccessGroup(Guid accountId, CreateAccessGroupBody body)
        {
            var group = await _accessRepository.CreateGroup(accountId, body);

            if (group == null)
            {
                return new ServiceResponse<AccessGroupBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new[] { "Не удалось создать группу доступа" }
                };
            }

            return new ServiceResponse<AccessGroupBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = group.ToAccessGroupBody()
            };
        }

        // Добавление пользователя в группу доступа
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

        // Удаление пользователя из группы доступа
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

        // Получение правил доступа для аккаунта и общие с ним 
        public async Task<ServiceResponse<AccessBody>> GetCommonAccessRules(Guid accountId)
        {
            var access = await _accessRepository.GetCommonAccessRules(accountId);

            if (access == null)
            {
                return new ServiceResponse<AccessBody>()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Body = new AccessBody()
                };
            }

            return new ServiceResponse<AccessBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access
            };
        }

        // Получение правил доступа для аккаунта
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

            return new ServiceResponse<AccessBody>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = access
            };
        }



        // Проверка доступа к ноде
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
