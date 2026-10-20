using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Api.Stubs;
using planner_content_service.App.Service;
using planner_content_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Http.Headers;

namespace planner_content_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IJwtService _jwtService;
        private readonly TestStubs _stubs;

        public TaskController(
            ITaskService taskService,
            IJwtService jwtService,
            TestStubs stubs)
        {
            _taskService = taskService;
            _jwtService = jwtService;
            _stubs = stubs;
        }

        [HttpPost("task"), Authorize]
        [SwaggerOperation("Создать/обновить задачу")]
        [SwaggerResponse(200, Type = typeof(JobBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> CreateOrUpdateTask(
            [FromBody] JobBodyRequest taskBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {

            //await Task.Delay(3000, cancellationToken);

            //var stub = _stubs.TryGet(TestStubKeys.DefaultVersionMismatch, out var s) ? s : null;

            //return StatusCode((int)HttpStatusCode.Conflict, new Response<BoardBody>() { PrimaryErrorCode = stub.Response, ErrorCodes = [stub.Response], Reason = stub.Reason });

            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.CreateOrUpdateTask(tokenPayload.AccountId, taskBody, cancellationToken);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<JobBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<JobBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("createJobFromMessage"), Authorize]
        [SwaggerOperation("Создать задачу от сообщения")]
        [SwaggerResponse(200, Type = typeof(JobBody))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]
        public async Task<IActionResult> CreateOrUpdateJobFromMessage(
            [FromBody] JobBodyRequest taskBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.CreateOrUpdateJobFromMessage(tokenPayload.AccountId, taskBody, cancellationToken);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPost("attachMessage"), Authorize]
        [SwaggerOperation("Привязать сообщение к задаче")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> AttachMessage(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] Guid messageId,
            [FromQuery] Guid jobId,
            [FromQuery] string snapshot,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.AttachMessage(tokenPayload.AccountId, jobId, messageId, snapshot, cancellationToken);
            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpPut("readAttachedMessages"), Authorize]
        [SwaggerOperation("Прочитать привязанные сообщения задачи")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> ReadAttachedMessages(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery] Guid jobId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _taskService.ReadAttachedMessages(tokenPayload.AccountId, jobId, cancellationToken);
            return StatusCode((int)result.StatusCode, result.Body);
        }
    }
}