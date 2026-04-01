using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_client_package.Entities.Enum;
using planner_client_package.Entities.Request;
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
            Request<CreateOrUpdateBoardBody> boardBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _idempotencyService.ExecuteOperation(
                boardBody.Id,
                tokenInfo.AccountId,
                OperationName.CreateOrUpdateBoard,
                boardBody.Body,
                async () => await _boardService.CreateOrUpdateBoardAsync(boardBody.Body, tokenInfo.AccountId)
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<BoardBody>() { RequestId = boardBody.Id, Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<BoardBody>() { RequestId = boardBody.Id, ErrorKind = result.ErrorKind });
        }

        [HttpPost("createBoards"), Authorize]
        [SwaggerOperation("Создать доски")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateOrUpdateBoards(
            List<CreateOrUpdateBoardBody> boardBodies,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _boardService.CreateOrUpdateBoards(boardBodies, tokenInfo.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("board/column"), Authorize]
        [SwaggerOperation("Создать колонку")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> CreateOrUpdateColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            ColumnBody column
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.CreateOrUpdateColumn(tokenPayload.AccountId, column);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpDelete("deleteNode"), Authorize]
        [SwaggerOperation("Удалить ноду")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> DeleteNode(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            Guid nodeId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.DeleteNode(tokenPayload.AccountId, nodeId);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("board/createColumns"), Authorize]
        [SwaggerOperation("Создать колонки")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> CreateOrUpdateColumns(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            List<ColumnBody> columns
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _boardService.CreateOrUpdateColumns(tokenPayload.AccountId, columns);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}