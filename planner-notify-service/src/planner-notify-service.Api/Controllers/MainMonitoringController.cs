using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_notify_service.Core.IService;

namespace planner_notify_service.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class MainMonitoringController : ControllerBase
    {
        private readonly IMainMonitoringConnector _mainMonitoringConnector;
        private readonly IJwtService _jwtService;

        public MainMonitoringController(
            IMainMonitoringConnector mainMonitoringConnector,
            IJwtService jwtService)
        {
            _mainMonitoringConnector = mainMonitoringConnector;
            _jwtService = jwtService;
        }

        [HttpGet("main"), Authorize]
        public async Task ConnectToMain([FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            if (!HttpContext.WebSockets.IsWebSocketRequest)
                return;

            var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _mainMonitoringConnector.ConnectToMainMonitoringService(tokenPayload.AccountId, tokenPayload.SessionId, ws);
        }
    }
}