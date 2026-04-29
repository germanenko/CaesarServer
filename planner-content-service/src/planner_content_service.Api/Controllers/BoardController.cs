using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_client_package.Entities.Enum;
using planner_client_package.Entities.Request;
using planner_common_package;
using planner_common_package.Entities;
using planner_content_service.Core.IService;
using planner_server_package.Idempotency.Interface;
using Swashbuckle.AspNetCore.Annotations;
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
        private readonly IIdempotencyService _idempotencyService;

        public BoardController(
            IBoardService boardService,
            IJwtService jwtService,
            IIdempotencyService idempotencyService)
        {
            _boardService = boardService;
            _jwtService = jwtService;
            _idempotencyService = idempotencyService;
        }

        [HttpPost("board"), Authorize]
        [SwaggerOperation("Создать доску")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        public async Task<IActionResult> CreateOrUpdateBoard(
            CreateOrUpdateBoardBody boardBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            CancellationToken cancellationToken
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenInfo.AccountId,
                OperationName.CreateOrUpdateBoard,
                boardBody,
                async () => await _boardService.CreateOrUpdateBoardAsync(boardBody, tokenInfo.AccountId, cancellationToken),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<BoardBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<BoardBody>() { ErrorCodes = result.ErrorCodes });
        }


        [HttpPost("board/column"), Authorize]
        [SwaggerOperation("Создать колонку")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> CreateOrUpdateColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            ColumnBodyRequest column,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.CreateOrUpdateColumn(tokenPayload.AccountId, column, cancellationToken);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpDelete("deleteNode"), Authorize]
        [SwaggerOperation("Удалить ноду")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> DeleteNode(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            Guid nodeId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.DeleteNode(tokenPayload.AccountId, nodeId, cancellationToken);
            return StatusCode((int)result.StatusCode, result.Body);
        }


        [HttpPost("addDefaultColumn"), Authorize]
        [SwaggerOperation("Добавить колонку по умолчанию")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> AddDefaultColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] TaskColumnRequest taskColumn,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.AddDefaultColumn(tokenPayload.AccountId, taskColumn, cancellationToken);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpGet("getDefaultColumns"), Authorize]
        [SwaggerOperation("Получить все колонки по умолчанию")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetDefaultColumns(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.GetDefaultColumns(tokenPayload.AccountId, cancellationToken);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}