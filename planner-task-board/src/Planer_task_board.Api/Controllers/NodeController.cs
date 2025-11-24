using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_task_board.Api.Controllers
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
            var result = await _nodeService.AddOrUpdateNode(tokenPayload.AccountId, node);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPost("createOrUpdateNodeLink"), Authorize]
        [SwaggerOperation("Создать или обновить связь")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> AddOrUpdateNodeLink(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromBody] NodeLink node
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _nodeService.AddOrUpdateNodeLink(tokenPayload.AccountId, node);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}