using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_task_board.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class DraftController : ControllerBase
    {
        private readonly IDraftService _draftService;
        private readonly IJwtService _jwtService;

        public DraftController(
            IDraftService draftService,
            IJwtService jwtService)
        {
            _draftService = draftService;
            _jwtService = jwtService;
        }

        [HttpPost("draft"), Authorize]
        [SwaggerOperation("Создать черновик")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> CreateDraft(
            [FromBody] CreateDraftBody draftBody,
            [FromQuery, Required] Guid columnId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _draftService.CreateDraft(draftBody, tokenPayload.AccountId, columnId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpGet("drafts"), Authorize]
        [SwaggerOperation("Получить черновики")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> GetDrafts(
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] Guid columnId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _draftService.GetDrafts(tokenPayload.AccountId, boardId, columnId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPut("draft/{draftId}"), Authorize]
        [SwaggerOperation("Преобразовать черновик в задачу")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> ConvertDraftToTask(
            [FromQuery, Required] Guid boardId,
            [Required] Guid draftId,
            [FromQuery, Required] Guid columnId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _draftService.ConvertDraftToTask(tokenPayload.AccountId, boardId, draftId, columnId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }


        [HttpPut("draft"), Authorize]
        [SwaggerOperation("Обновить черновик")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> UpdateDraft(
            UpdateDraftBody draftBody,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _draftService.UpdateDraft(tokenPayload.AccountId, boardId, draftBody.Id, draftBody);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }
    }
}