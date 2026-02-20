using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
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

        public NodeController(
            INodeService nodeService)
        {
            _nodeService = nodeService;
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
    }
}