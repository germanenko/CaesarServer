using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_common_package;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Api.Stubs;
using planner_content_service.App.Service;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IService;
using planner_server_package.Idempotency.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http.Headers;

namespace planner_content_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class NodeController : ControllerBase
    {
        private readonly INodeService _nodeService;
        private readonly IJwtService _jwtService;
        private readonly IIdempotencyService _idempotencyService;
        private readonly TestStubs _stubs;

        public NodeController(
            INodeService nodeService, IJwtService jwtService, IIdempotencyService idempotencyService, TestStubs stubs)
        {
            _nodeService = nodeService;
            _jwtService = jwtService;
            _idempotencyService = idempotencyService;
            _stubs = stubs;
        }

        [HttpGet("content/getNodesByIds")]
        [SwaggerOperation("Получить ноды по ID")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetNodesByIds(
            [FromQuery] List<Guid> nodeIds
        )
        {
            var result = await _nodeService.GetNodesByIds(nodeIds);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpDelete("deleteNode"), Authorize]
        [SwaggerOperation("Удалить ноду")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> DeleteNode(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            Guid nodeId,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            CancellationToken cancellationToken
        )
        {
            //await System.Threading.Tasks.Task.Delay(3000);

            //var stub = _stubs.TryGet(TestStubKeys.NodeDeleteWriteDenied, out var s) ? s : null;

            //return StatusCode(403, new Response<BoardBody>() { PrimaryErrorCode = stub.Response, ErrorCodes = [stub.Response], Reason = stub.Reason });

            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _nodeService.DeleteNode(tokenPayload.AccountId, nodeId, cancellationToken);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.DeleteNode,
                nodeId,
                async () => await _nodeService.DeleteNode(tokenPayload.AccountId, nodeId, cancellationToken),
                cancellationToken
            );


            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<bool>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<bool>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

    }
}