using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;

namespace planner_content_service.Api.Controllers
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
            BoardBody boardBody,
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
            List<BoardBody> boardBodies,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _boardService.CreateBoardsAsync(boardBodies, tokenInfo.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("board/column"), Authorize]
        [SwaggerOperation("Создать колонку")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            ColumnBody column
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
            List<ColumnBody> columns
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.AddColumns(tokenPayload.AccountId, columns);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}