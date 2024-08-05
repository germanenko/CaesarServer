using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
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

        [HttpGet("boards"), Authorize]
        [SwaggerOperation("Получить список досок")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetBoards(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _boardService.GetBoardsAsync(tokenInfo.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpGet("board/columns"), Authorize]
        [SwaggerOperation("Получить список колоннок")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<BoardColumnBody>))]

        public async Task<IActionResult> GetColumnsByBoard(
            [FromQuery, Required] Guid boardId
        )
        {
            var result = await _boardService.GetBoardColumnsAsync(boardId);
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
            [FromQuery, Required] Guid accountId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.AddBoardMemberAsync(boardId, tokenPayload.AccountId, accountId);
            return StatusCode((int)result);
        }

        [HttpPost("board/column"), Authorize]
        [SwaggerOperation("Создать колонку")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] string name
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.AddColumn(tokenPayload.AccountId, boardId, name);
            return StatusCode((int)result);
        }
    }
}