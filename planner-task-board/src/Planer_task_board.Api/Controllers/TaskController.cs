using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_task_board.Api.Controllers
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
        [SwaggerOperation("Создать задачу")]
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> CreateTask(
            [FromBody] CreateTaskBody taskBody,
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] Guid columnId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.CreateTask(tokenPayload.AccountId, boardId, columnId, taskBody);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpDelete("task")]
        [SwaggerOperation("Удалить задачу")]
        [SwaggerResponse(200, Type = typeof(DeletedTaskBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> RemoveTask(
            [FromQuery, Required] Guid taskId,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.RemoveTask(tokenPayload.AccountId, boardId, taskId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPatch("task")]
        [SwaggerOperation("Восстановить удаленную задачу")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> RemoveDraft(
            [Required] Guid deletedTaskId,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.RestoreDeletedTask(deletedTaskId, boardId, tokenPayload.AccountId);
            return StatusCode((int)result);
        }

        [HttpGet("deleted-tasks")]
        [SwaggerOperation("Получить все удаленные задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<DeletedTaskBody>))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> GetDrafts(
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.GetDeletedTasks(tokenPayload.AccountId, boardId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }



        [HttpGet("tasks"), Authorize]
        [SwaggerOperation("Получить задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<TaskBody>))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> GetTasks(
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] Guid columnId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery] TaskState? status = null
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.GetTasks(tokenPayload.AccountId, boardId, columnId, status);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpGet("task/performers"), Authorize]
        [SwaggerOperation("Получить список исполнителей задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<Guid>))]

        public async Task<IActionResult> GetTaskPerformers(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid taskId,
            [FromQuery, Required] Guid boardId,
            [FromQuery] int count = 1,
            [FromQuery] int offset = 0
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.GetTaskPerformerIds(tokenPayload.AccountId, boardId, taskId, count, offset);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("task/performers"), Authorize]
        [SwaggerOperation("Добавить исполнителей задачи")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<Guid>))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> AddTaskPerformers(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid taskId,
            [FromQuery, Required] Guid boardId,
            [FromBody] IEnumerable<Guid> userIds
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.AddTaskPerformers(tokenPayload.AccountId, taskId, boardId, userIds);
            return StatusCode((int)result);
        }


        [HttpPut("task"), Authorize]
        [SwaggerOperation("Обновить задачу")]
        [SwaggerResponse(200, Type = typeof(TaskBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> UpdateTask(
            UpdateTaskBody taskBody,
            [FromQuery, Required] Guid boardId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.UpdateTask(tokenPayload.AccountId, boardId, taskBody);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("task/column"), Authorize]
        [SwaggerOperation("Добавить задачу в колонку любой доски")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]
        public async Task<IActionResult> AddTaskToColumn(
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] Guid columnId,
            [FromQuery, Required] Guid taskId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.AddTaskToColumn(tokenPayload.AccountId, boardId, taskId, columnId);
            return StatusCode((int)result);
        }

        [HttpDelete("task/column"), Authorize]
        [SwaggerOperation("Удалить задачу из колонки")]
        [SwaggerResponse(204)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]
        public async Task<IActionResult> RemoveTaskFromColumn(
            [FromQuery, Required] Guid boardId,
            [FromQuery, Required] Guid columnId,
            [FromQuery, Required] Guid taskId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.RemoveTaskFromColumn(tokenPayload.AccountId, boardId, taskId, columnId);
            return StatusCode((int)result);
        }
    }
}