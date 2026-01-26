using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_chat_service.Api.CustomAttributes;
using planner_chat_service.Core.Entities.Request;
using planner_chat_service.Core.IService;
using planner_client_package.Entities;
using planner_common_package.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace planner_chat_service.Api.Controllers
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


        [SwaggerOperation("Дублирование письма в чат")]
        [LocalOnly]
        [HttpPost("api/sendFromEmail")]
        [SwaggerResponse(200, Type = typeof(Guid))]
        [SwaggerResponse(404)]
        public async Task<IActionResult> SendMessageFromEmail(
            [FromQuery] Guid senderId,
            [FromQuery] Guid receiverId,
            [FromQuery] string content
        )
        {
            var result = await _chatService.SendMessage(senderId, receiverId, content);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
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

        [HttpGet("api/chat"), Authorize]
        [SwaggerOperation("Получить чат")]
        [SwaggerResponse(200, Type = typeof(ChatBody))]

        public async Task<IActionResult> GetChat(
            [FromQuery] Guid chatId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetChat(tokenInfo.AccountId, tokenInfo.SessionId, chatId);
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
            [FromBody, Required] ChatBody chatBody,
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

        [HttpPut("api/chat/editMessage")]
        [SwaggerOperation("Редактировать сообщение")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageBody>))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> EditMessage(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromBody, Required] MessageBody updatedMessage
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.EditMessage(tokenPayload.AccountId, updatedMessage);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpGet("api/chat/allMessages")]
        [SwaggerOperation("Получить все сообщения")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageBody>))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> GetAllMessages(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetAllMessages(tokenPayload.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("api/createOrUpdateMessageDraft"), Authorize]
        [SwaggerOperation("Создать/обновить черновик сообщения")]
        [SwaggerResponse(200, Type = typeof(bool))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> CreateOrUpdateMessageDraft(
            [FromBody, Required] Guid chatId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            string content
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.CreateOrUpdateMessageDraft(tokenPayload.AccountId, chatId, content);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("api/createOrUpdateMessageDrafts"), Authorize]
        [SwaggerOperation("Создать/обновить список черновиков")]
        [SwaggerResponse(200, Type = typeof(bool))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> CreateOrUpdateMessageDrafts(
            [FromBody, Required] List<MessageDraftBody> drafts,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.CreateOrUpdateMessageDrafts(tokenPayload.AccountId, drafts);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpGet("api/getMessageDraft")]
        [SwaggerOperation("Получить черновик сообщения")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageDraftBody>))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> GetMessageDraft(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromBody, Required] Guid chatId
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetMessageDraft(tokenPayload.AccountId, chatId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("api/enableNotifications"), Authorize]
        [SwaggerOperation("Включить/отключить уведомления от чата")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageDraftBody>))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> EnableNotifications(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery] Guid chatId,
            [FromQuery] bool enable
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.SetEnabledNotifications(tokenPayload.AccountId, chatId, enable);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        [HttpGet("api/getChatsSettings"), Authorize]
        [SwaggerOperation("Получить настройки чатов")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageDraftBody>))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> GetChatsSettings(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetChatsSettings(tokenPayload.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }
    }
}