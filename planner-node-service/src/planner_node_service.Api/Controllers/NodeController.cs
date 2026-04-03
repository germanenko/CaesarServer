using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_node_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
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

        [HttpGet("getNodesByIds"), Authorize]
        [SwaggerOperation("Получить ноды по Id")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetNodesByIds(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] List<Guid> nodeIds
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetNodesByIds(tokenPayload.AccountId, nodeIds);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpGet("getNodes"), Authorize]
        [SwaggerOperation("Получить ноды")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetNodes(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetNodes(tokenPayload.AccountId);
            return StatusCode((int)result.StatusCode, result.Body);
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

        [HttpGet("getNodeLinks"), Authorize]
        [SwaggerOperation("Получить ссылки")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetNodeLinks(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.GetNodeLinks(tokenPayload.AccountId);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("createOrUpdateNode"), Authorize]
        [SwaggerOperation("Создать или обновить ноду")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddOrUpdateNode(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] NodeBody node
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.AddOrUpdateNode(node);
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

        [HttpPost("createOrUpdateNodeLink"), Authorize]
        [SwaggerOperation("Создать или обновить ссылку")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddOrUpdateNodeLink(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] NodeLinkBody node
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.AddOrUpdateNodeLink(node);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("createOrUpdateNodeLinks"), Authorize]
        [SwaggerOperation("Создать или обновить ссылки")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddOrUpdateNodeLinks(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] List<NodeLinkBody> nodes
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.AddOrUpdateNodeLinks(nodes);
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