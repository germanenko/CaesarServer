using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_node_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Http.Headers;

namespace planner_node_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class NodeController : ControllerBase
    {
        private readonly INodeService _nodeService;
        private readonly IJwtService _jwtService;

        public NodeController(
            INodeService nodeService,
            IJwtService jwtService)
        {
            _nodeService = nodeService;
            _jwtService = jwtService;
        }

        [HttpGet("getNodeBranchesByRootIds"), Authorize]
        [SwaggerOperation("Получить ветки нод по Id корней")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetNodeBranchesByRootIds(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] List<Guid> rootIds
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetNodes(tokenPayload.AccountId, rootIds);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpGet("getNodesByIds"), Authorize]
        [SwaggerOperation("Получить ноды по Id")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetNodesByIds(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] List<Guid> nodeIds
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _nodeService.GetNodesByIds(tokenPayload.AccountId, nodeIds);
            //return StatusCode((int)result.StatusCode, result.Body); убрал для тестов
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpGet("getManifest"), Authorize]
        [SwaggerOperation("Получить манифест")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetManifest(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] List<Guid> scopeIds
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetManifests(tokenPayload.AccountId, scopeIds);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpGet("getScopesManifest"), Authorize]
        [SwaggerOperation("Получить Scopes манифест")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetScopesManifest(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetScopesManifest(tokenPayload.AccountId);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("changeNodeParent"), Authorize]
        [SwaggerOperation("Изменить родителя ноды")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> ChangeNodeParent(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] Guid nodeId,
            [FromQuery] Guid newParentId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.ChangeNodeParent(tokenPayload.AccountId, nodeId, newParentId);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("sendLocalNodes"), Authorize]
        [SwaggerOperation("Отправить локальные ноды")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> SendLocalNodes(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] List<NodeBody> nodes
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.LoadNodes(nodes, tokenPayload);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}