using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_content_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
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
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> CreateOrUpdateTask(
            [FromBody] TaskBody taskBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.CreateOrUpdateTask(tokenPayload.AccountId, taskBody);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("createOrUpdateTasks"), Authorize]
        [SwaggerOperation("Создать/обновить задачи")]
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> CreateOrUpdateTasks(
            [FromBody] List<TaskBody> taskBodies,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.CreateOrUpdateTasks(tokenPayload.AccountId, taskBodies);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }
    }
}