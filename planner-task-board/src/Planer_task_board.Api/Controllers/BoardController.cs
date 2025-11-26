using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_task_board.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _boardService;
        private readonly IJwtService _jwtService;

        public BoardController(
            IBoardService boardService,
            IJwtService jwtService)
        {
            _boardService = boardService;
            _jwtService = jwtService;
        }

        [HttpPost("board"), Authorize]
        [SwaggerOperation("Создать доску")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateBoard(
            CreateBoardBody boardBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _boardService.CreateBoardAsync(boardBody, tokenInfo.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("createBoards"), Authorize]
        [SwaggerOperation("Создать доски")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateBoards(
            List<CreateBoardBody> boardBodies,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _boardService.CreateBoardsAsync(boardBodies, tokenInfo.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }


        [HttpGet("board/members"), Authorize]
        [SwaggerOperation("Получить список участников доски")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<Guid>))]
        public async Task<IActionResult> GetBoardMembers(
            [FromQuery, Required] Guid boardId,
            [FromQuery] int count = 1,
            [FromQuery] int offset = 0
        )
        {
            var result = await _boardService.GetBoardMembersAsync(boardId, count, offset);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("board/member"), Authorize]
        [SwaggerOperation("Добавить участника доски")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]

        public async Task<IActionResult> AddMember(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] Guid accountId,
            [FromQuery, Required] AccessType accessType
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.AddBoardMemberAsync(boardId, tokenPayload.AccountId, accountId, accessType);
            return StatusCode((int)result);
        }

        [HttpPost("board/column"), Authorize]
        [SwaggerOperation("Создать колонку")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            CreateColumnBody column
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.AddColumn(tokenPayload.AccountId, column);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("board/createColumns"), Authorize]
        [SwaggerOperation("Создать колонки")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddColumns(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            List<CreateColumnBody> columns
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.AddColumns(tokenPayload.AccountId, columns);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}