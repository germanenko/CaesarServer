using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planner_chat_server.Core.Entities.Request;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;
using Planner_chat_server.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planner_chat_server.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class ChatController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IChatService _chatService;

        public ChatController(IJwtService jwtService,
                              IChatService chatService)
        {
            _jwtService = jwtService;
            _chatService = chatService;
        }

        [HttpGet("chat"), Authorize]
        [SwaggerOperation("Подключиться к чату")]
        public async Task ConnectToChat(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, Required] Guid chatId
        )
        {
            var websocketManager = HttpContext.WebSockets;
            if (!websocketManager.IsWebSocketRequest)
                return;

            var tokenPayload = _jwtService.GetTokenPayload(token);
            var ws = await websocketManager.AcceptWebSocketAsync();

            await _chatService.ConnectToChat(tokenPayload.AccountId, chatId, ws, tokenPayload.SessionId);
        }


        [HttpGet("api/chats"), Authorize]
        [SwaggerOperation("Получить список чатов")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ChatBody>))]

        public async Task<IActionResult> GetChats(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetChats(tokenInfo.AccountId, tokenInfo.SessionId, ChatType.Personal);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("api/chat"), Authorize]
        [SwaggerOperation("Создать личный чат")]
        [SwaggerResponse(200, Type = typeof(Guid))]
        [SwaggerResponse(404)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> CreatePersonalChat(
            [FromBody, Required] CreateChatBody chatBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, Required] Guid addedAccountId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.CreatePersonalChat(tokenPayload.AccountId, tokenPayload.SessionId, chatBody, addedAccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("api/chat/messages")]
        [SwaggerOperation("Получить список сообщений")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageBody>))]
        [SwaggerResponse(403)]

        public async Task<IActionResult> GetMessages(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid chatId,
            [FromBody, Required] DynamicDataLoadingOptions loadingOptions
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetMessages(tokenPayload.AccountId, chatId, loadingOptions);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }
    }
}