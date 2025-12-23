using CaesarServerLibrary.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_node_service.Core.Entities.Models;
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

        [HttpGet("getNodeLinks"), Authorize]
        [SwaggerOperation("Получить связи")]
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
            [FromBody] Node node
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.AddOrUpdateNode(node);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("createOrUpdateNodeLink"), Authorize]
        [SwaggerOperation("Создать или обновить связь")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddOrUpdateNodeLink(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] CreateOrUpdateNodeLink node
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.AddOrUpdateNodeLink(node);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("createOrUpdateNodeLinks"), Authorize]
        [SwaggerOperation("Создать или обновить связи")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddOrUpdateNodeLinks(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] List<CreateOrUpdateNodeLink> nodes
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.AddOrUpdateNodeLinks(nodes);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}