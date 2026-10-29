using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Api.Stubs;
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
        private readonly TestStubs _stubs;

        public BoardController(
            IBoardService boardService,
            IJwtService jwtService,
            IIdempotencyService idempotencyService,
            TestStubs stubs)
        {
            _boardService = boardService;
            _jwtService = jwtService;
            _idempotencyService = idempotencyService;
            _stubs = stubs;
        }

        [HttpPost("board"), Authorize]
        [SwaggerOperation("Создать/обновить доску")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        public async Task<IActionResult> CreateOrUpdateBoard(
            CreateOrUpdateBoardBody boardBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            CancellationToken cancellationToken
        )
        {
            await Task.Delay(1000);

            var stub = _stubs.TryGet(TestStubKeys.BoardUpdateWriteDenied, out var s) ? s : null;

            return StatusCode((int)HttpStatusCode.Forbidden, new Response<BoardBody>() { PrimaryErrorCode = stub.Response, ErrorCodes = [stub.Response], Reason = stub.Reason });

            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenInfo.AccountId,
                OperationName.CreateOrUpdateBoard,
                boardBody,
                async () => await _boardService.CreateOrUpdateBoardAsync(requestId, boardBody, tokenInfo.AccountId, cancellationToken),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<BoardBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<BoardBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }


        [HttpPost("board/column"), Authorize]
        [SwaggerOperation("Создать/обновить колонку")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> CreateOrUpdateColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            ColumnBodyRequest column,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.CreateOrUpdateColumn,
                column,
                async () => await _boardService.CreateOrUpdateColumn(requestId, tokenPayload.AccountId, column, cancellationToken),
                cancellationToken
            );

            //var result = await _boardService.CreateOrUpdateColumn(tokenPayload.AccountId, column, cancellationToken);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<ColumnBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<ColumnBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("addDefaultColumn"), Authorize]
        [SwaggerOperation("Добавить колонку по умолчанию")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> AddDefaultColumn(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] TaskColumnRequest taskColumn,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _boardService.AddDefaultColumn(tokenPayload.AccountId, taskColumn, cancellationToken);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.AddDefaultAttachColumn,
                taskColumn,
                async () => await _boardService.AddDefaultColumn(tokenPayload.AccountId, taskColumn, cancellationToken),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<TaskColumnBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<TaskColumnBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
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

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<TaskColumnBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<TaskColumnBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }
    }
}