using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_task_board.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AccessController : ControllerBase
    {
        private readonly IAccessService _accessService;
        private readonly IJwtService _jwtService;

        public AccessController(
            IAccessService accessService,
            IJwtService jwtService)
        {
            _accessService = accessService;
            _jwtService = jwtService;
        }

        [HttpPost("createOrUpdateAccessGroup"), Authorize]
        [SwaggerOperation("Создать группу доступа")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> CreateOrUpdateAccessGroup(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] CreateAccessGroupBody body
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _accessService.CreateOrUpdateAccessGroup(tokenPayload.AccountId, body);
            return StatusCode((int)result.StatusCode, result.Body);
        }


        [HttpPut("addUserToGroup"), Authorize]
        [SwaggerOperation("Добавить пользователя в группу")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> AddUserToGroup(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] Guid userId,
            [FromQuery] Guid groupId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _accessService.AddUserToGroup(tokenPayload.AccountId, userId, groupId);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpDelete("removeUserFromGroup"), Authorize]
        [SwaggerOperation("Удалить пользователя из группы")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> RemoveUserFromGroup(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] Guid userId,
            [FromQuery] Guid groupId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _accessService.RemoveUserFromGroup(tokenPayload.AccountId, userId, groupId);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}