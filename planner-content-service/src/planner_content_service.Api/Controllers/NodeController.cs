using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_content_service.App.Service;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IService;
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

        public NodeController(
            INodeService nodeService, IJwtService jwtService)
        {
            _nodeService = nodeService;
            _jwtService = jwtService;
        }

        [HttpGet("content/getNodesByIds")]
        [SwaggerOperation("Получить ноды по ID")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetNodesByIds(
            [FromQuery] List<Guid> nodeIds
        )
        {
            var result = await _nodeService.GetNodesByIds(nodeIds);
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
            await System.Threading.Tasks.Task.Delay(3000);

            return StatusCode(403, new Response<BoardBody>() { PrimaryErrorCode = ErrorCode.WriteDenied, ErrorCodes = [ErrorCode.WriteDenied] });

            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.DeleteNode(tokenPayload.AccountId, nodeId, cancellationToken);
            return StatusCode((int)result.StatusCode, new Response<bool>() { Body = result.Body });
        }

    }
}