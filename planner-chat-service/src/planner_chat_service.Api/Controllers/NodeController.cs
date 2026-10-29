using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_chat_service.Core.Entities.Models;
using planner_chat_service.Core.IService;
using planner_client_package.Entities;
using planner_common_package.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http.Headers;

namespace planner_chat_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class NodeController : ControllerBase
    {
        private readonly INodeService _nodeService;

        public NodeController(
            INodeService nodeService)
        {
            _nodeService = nodeService;
        }

        [HttpGet("chat/getNodesByIds")]
        [SwaggerOperation("Получить ноды по ID")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetNodesByIds(
            [FromQuery] List<Guid> nodeIds,
            [FromQuery] Guid accountId
        )
        {
            var result = await _nodeService.GetNodesByIds(accountId, nodeIds);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<IEnumerable<NodeBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }
    }
}