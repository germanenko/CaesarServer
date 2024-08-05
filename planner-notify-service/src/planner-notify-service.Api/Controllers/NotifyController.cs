using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_notify_service.Core.IService;

namespace planner_notify_service.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class NotifyController : ControllerBase
    {
        private readonly INotificationConnector _notifyConnector;
        private readonly IJwtService _jwtService;

        public NotifyController(
            INotificationConnector notifyConnector,
            IJwtService jwtService)
        {
            _notifyConnector = notifyConnector;
            _jwtService = jwtService;
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
    }
}