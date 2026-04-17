using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_content_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace planner_content_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IJwtService _jwtService;

        public TaskController(
            ITaskService taskService,
            IJwtService jwtService)
        {
            _taskService = taskService;
            _jwtService = jwtService;
        }

        [HttpPost("task"), Authorize]
        [SwaggerOperation("Создать/обновить задачу")]
        [SwaggerResponse(200, Type = typeof(JobBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> CreateOrUpdateTask(
            [FromBody] JobBodyRequest taskBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.CreateOrUpdateTask(tokenPayload.AccountId, taskBody);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("createTaskFromMessage"), Authorize]
        [SwaggerOperation("Создать задачу от сообщения")]
        [SwaggerResponse(200, Type = typeof(JobBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]
        public async Task<IActionResult> CreateJobFromMessage(
            [FromQuery] string snapshot,
            [FromQuery] Guid messageId,
            [FromBody] JobBodyRequest taskBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.CreateTaskFromMessage(tokenPayload.AccountId, taskBody, snapshot, messageId);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }
    }
}