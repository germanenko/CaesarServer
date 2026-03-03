using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_analytics_service.Core.IService;
using planner_client_package.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace planner_analytics_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(
            IJwtService jwtService,
            IAnalyticsService analyticsService)
        {
            _jwtService = jwtService;
            _analyticsService = analyticsService;
        }

        [HttpPost("addAnalyticsAction"), Authorize]
        [SwaggerOperation("Добавить действие для аналитики")]
        [SwaggerResponse(200, Type = typeof(Guid))]
        [SwaggerResponse(404)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> AddAnalyticsAction(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromBody] AnalyticsActionBody action
        )
        {
            var result = await _analyticsService.AddAction(action);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("addAnalyticsActions"), Authorize]
        [SwaggerOperation("Добавить действия для аналитики")]
        [SwaggerResponse(200, Type = typeof(Guid))]
        [SwaggerResponse(404)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> AddAnalyticsActions(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromBody] List<AnalyticsActionBody> actions
        )
        {
            var result = await _analyticsService.AddActions(actions);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }
    }
}