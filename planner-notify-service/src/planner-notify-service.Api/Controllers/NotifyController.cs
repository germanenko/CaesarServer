using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_notify_service.Api.CustomAttributes;
using planner_notify_service.App.Service;
using planner_notify_service.Core.Entities.Models;
using planner_notify_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace planner_notify_service.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class NotifyController : ControllerBase
    {
        private readonly INotificationConnector _notifyConnector;
        private readonly IJwtService _jwtService;
        private readonly INotifyService _notifyService;

        public NotifyController(
            INotificationConnector notifyConnector,
            IJwtService jwtService,
            INotifyService notifyService)
        {
            _notifyConnector = notifyConnector;
            _jwtService = jwtService;
            _notifyService = notifyService;
        }

        [HttpGet("notify"), Authorize]
        public async Task ConnectToNotify([FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return;

            var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _notifyConnector.ConnectToNotificationService(tokenPayload.AccountId, tokenPayload.SessionId, ws);
        }


        [HttpPost("api/addFirebaseToken"), Authorize]
        [SwaggerOperation("Добавить Firebase токен")]
        [SwaggerResponse(200, Type = typeof(FirebaseToken))]

        public async Task<IActionResult> AddFirebaseToken(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] string firebaseToken
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var response = await _notifyService.AddFirebaseToken(tokenInfo.AccountId, firebaseToken);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }

        [HttpPost("api/sendFCMNotification")]
        [SwaggerOperation("Добавить Firebase токен")]
        [SwaggerResponse(200, Type = typeof(bool))]

        public async Task<IActionResult> SendFCMNotification(
            [FromQuery, Required] string firebaseToken,
            [FromQuery] string title,
            [FromQuery] string content
        )
        {
            var response = await _notifyService.SendFCMNotification(firebaseToken, title, content);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }
    }
}