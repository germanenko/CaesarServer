using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_client_package.Interface;
using planner_common_package;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Api.CustomAttributes;
using planner_node_service.Core.IService;
using planner_server_package.Events;
using planner_server_package.Idempotency.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;

namespace planner_node_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AccessController : ControllerBase
    {
        private readonly IAccessService _accessService;
        private readonly IJwtService _jwtService;
        private readonly IIdempotencyService _idempotencyService;

        public AccessController(
            IAccessService accessService,
            IIdempotencyService idempotencyService,
            IJwtService jwtService)
        {
            _accessService = accessService;
            _jwtService = jwtService;
            _idempotencyService = idempotencyService;
        }

        [LocalOnly]
        [HttpGet("checkAccess")]
        [SwaggerOperation("Проверить доступ")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> CheckAccess(
            [FromQuery] Guid accountId,
            [FromQuery] Guid nodeId,
            [FromQuery] Permission minRequiredPermission,
            CancellationToken cancellationToken
        )
        {
            var result = await _accessService.CheckAccess(accountId, nodeId, minRequiredPermission);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<bool>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<bool>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("grantAccess"), Authorize]
        [SwaggerOperation("Выдать доступ")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GrantAccess(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromQuery] Guid accountId,
            [FromQuery] Guid nodeId,
            [FromQuery] Permission permission,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _accessService.GrantAccess(tokenPayload.AccountId, accountId, nodeId, permission);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.GrantAccess,
                (accountId, nodeId, permission),
                async () => await _accessService.GrantAccess(tokenPayload.AccountId, accountId, nodeId, permission),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<AccessRuleBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<AccessRuleBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("changePermission"), Authorize]
        [SwaggerOperation("Изменить уровень доступа")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> ChangePermission(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromQuery] Guid accountId,
            [FromQuery] Guid nodeId,
            [FromQuery] Permission permission,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _accessService.ChangePermission(tokenPayload.AccountId, accountId, nodeId, permission);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.ChangePermission,
                (accountId, nodeId, permission),
                async () => await _accessService.ChangePermission(tokenPayload.AccountId, accountId, nodeId, permission),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<AccessRuleBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<AccessRuleBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("revokeAccess"), Authorize]
        [SwaggerOperation("Отозвать доступ")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> RevokeAccess(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromQuery] Guid accountId,
            [FromQuery] Guid nodeId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _accessService.RevokeAccess(tokenPayload.AccountId, accountId, nodeId);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.RevokeAccess,
                (accountId, nodeId),
                async () => await _accessService.RevokeAccess(tokenPayload.AccountId, accountId, nodeId),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<bool>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<bool>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("createOrUpdateAccessGroup"), Authorize]
        [SwaggerOperation("Создать группу доступа")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> CreateOrUpdateAccessGroup(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromBody] CreateAccessGroupBody body,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _accessService.CreateAccessGroup(tokenPayload.AccountId, body);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.CreateOrUpdateAccessGroup,
                body,
                async () => await _accessService.CreateAccessGroup(tokenPayload.AccountId, body),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<GroupAccessSubjectBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<GroupAccessSubjectBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }


        [HttpPut("addUserToGroup"), Authorize]
        [SwaggerOperation("Добавить пользователя в группу")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> AddUserToGroup(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromQuery] Guid userId,
            [FromQuery] Guid groupId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _accessService.AddUserToGroup(tokenPayload.AccountId, userId, groupId);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.AddUserToGroup,
                (userId, groupId),
                async () => await _accessService.AddUserToGroup(tokenPayload.AccountId, userId, groupId),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<AccessGroupMemberBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<AccessGroupMemberBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpDelete("removeUserFromGroup"), Authorize]
        [SwaggerOperation("Удалить пользователя из группы")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> RemoveUserFromGroup(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromQuery] Guid userId,
            [FromQuery] Guid groupId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _accessService.RemoveUserFromGroup(tokenPayload.AccountId, userId, groupId);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.RemoveUserFromGroup,
                (userId, groupId),
                async () => await _accessService.RemoveUserFromGroup(tokenPayload.AccountId, userId, groupId),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<HttpStatusCode>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<HttpStatusCode>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("getAccessRules"), Authorize]
        [SwaggerOperation("Получить список прав доступа")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetAccessRules(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _accessService.GetAccessRules(tokenPayload.AccountId);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<AccessRuleBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<AccessRuleBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("getCommonAccessRules"), Authorize]
        [SwaggerOperation("Получить общие права доступа")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetCommonAccessRules(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _accessService.GetCommonAccessRules(tokenPayload.AccountId);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<AccessRuleBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<AccessRuleBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }
    }
}