using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql.Replication.PgOutput.Messages;
using planner_chat_service.Api.CustomAttributes;
using planner_chat_service.Core.Entities.Models;
using planner_chat_service.Core.Entities.Request;
using planner_chat_service.Core.IService;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_server_package.Idempotency;
using planner_server_package.Idempotency.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Threading;
using static Google.Rpc.Context.AttributeContext.Types;

namespace planner_chat_service.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class ChatController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IChatService _chatService;
        private readonly IIdempotencyService _idempotencyService;

        public ChatController(IJwtService jwtService,
                              IChatService chatService,
                              IIdempotencyService idempotencyService)
        {
            _jwtService = jwtService;
            _chatService = chatService;
            _idempotencyService = idempotencyService;
        }

        [HttpGet("chat"), Authorize]
        [SwaggerOperation("Подключиться к чату")]
        public async Task ConnectToChat(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader, Required] Guid chatId,
            CancellationToken cancellationToken
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
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromQuery] Guid senderId,
            [FromQuery] Guid receiverId,
            [FromQuery] string content,
            CancellationToken cancellationToken
        )
        {
            //var result = await _chatService.SendMessage(senderId, null, receiverId, content);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                senderId,
                OperationName.SendToMail,
                (senderId, receiverId, content),
                async () => await _chatService.SendMessage(senderId, null, receiverId, content),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<MessageBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<MessageBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }


        [HttpGet("api/chats"), Authorize]
        [SwaggerOperation("Получить список чатов")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ChatBody>))]

        public async Task<IActionResult> GetChats(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetChats(tokenInfo.AccountId, tokenInfo.SessionId, ChatType.Personal);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<IEnumerable<ChatBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<IEnumerable<ChatBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("api/chat"), Authorize]
        [SwaggerOperation("Получить чат")]
        [SwaggerResponse(200, Type = typeof(ChatBody))]

        public async Task<IActionResult> GetChat(
            [FromQuery] Guid chatId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetChat(tokenInfo.AccountId, tokenInfo.SessionId, chatId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<ChatBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<ChatBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("api/chat"), Authorize]
        [SwaggerOperation("Создать личный чат")]
        [SwaggerResponse(200, Type = typeof(Guid))]
        [SwaggerResponse(404)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> CreatePersonalChat(
            [FromBody, Required] CreateChatBody createChatBody,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.CreatePersonalChat,
                JsonSerializer.Serialize(createChatBody),
                async () => await _chatService.CreatePersonalChat(
                    tokenPayload.AccountId,
                    tokenPayload.SessionId,
                    createChatBody),
                cancellationToken
                );

            //var result = await _chatService.CreatePersonalChat(
            //        tokenPayload.AccountId,
            //        tokenPayload.SessionId,
            //        createChatBody);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<ChatBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<ChatBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("api/chat/messages")]
        [SwaggerOperation("Получить список сообщений")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageBody>))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> GetMessages(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid chatId,
            [FromBody, Required] DynamicDataLoadingOptions loadingOptions,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetMessages(tokenPayload.AccountId, chatId, loadingOptions);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<IEnumerable<MessageBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<IEnumerable<MessageBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPut("api/chat/editMessage")]
        [SwaggerOperation("Редактировать сообщение"), Authorize]
        [SwaggerResponse(200, Type = typeof(MessageBody))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> EditMessage(
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromBody, Required] EditMessageBody updatedMessage,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _chatService.EditMessage(tokenPayload.AccountId, updatedMessage);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.EditMessage,
                updatedMessage,
                async () => await _chatService.EditMessage(tokenPayload.AccountId, updatedMessage),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<MessageBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<MessageBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPut("api/chat/deleteMessageForMe"), Authorize]
        [SwaggerOperation("Удалить сообщение у меня")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageBody>))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> DeleteMessageForMe(
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid messageId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _chatService.DeleteMessageForMe(tokenPayload.AccountId, messageId);

            var result = await _idempotencyService.ExecuteOperation(
                requestId,
                tokenPayload.AccountId,
                OperationName.DeleteMessageForMe,
                messageId,
                async () => await _chatService.DeleteMessageForMe(tokenPayload.AccountId, messageId),
                cancellationToken
            );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<MessageBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<MessageBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPut("api/chat/deleteMessage"), Authorize]
        [SwaggerOperation("Удалить сообщение у всех")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<MessageBody>))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> DeleteMessage(
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid messageId,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _chatService.DeleteMessage(tokenPayload.AccountId, messageId);

            var result = await _idempotencyService.ExecuteOperation(
               requestId,
               tokenPayload.AccountId,
               OperationName.DeleteMessage,
               messageId,
               async () => await _chatService.DeleteMessage(tokenPayload.AccountId, messageId),
               cancellationToken
           );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<MessageBody>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<MessageBody>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpPost("api/createOrUpdateMessageDraft"), Authorize]
        [SwaggerOperation("Создать/обновить черновик сообщения")]
        [SwaggerResponse(200, Type = typeof(bool))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> CreateOrUpdateMessageDraft(
            [FromHeader(Name = RequestHeader.RequestId)] Guid requestId,
            [FromBody, Required] Guid chatId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            string content,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            //var result = await _chatService.CreateOrUpdateMessageDraft(tokenPayload.AccountId, chatId, content);

            var result = await _idempotencyService.ExecuteOperation(
               requestId,
               tokenPayload.AccountId,
               OperationName.CreateOrUpdateMessageDraft,
               (chatId, content),
               async () => await _chatService.CreateOrUpdateMessageDraft(tokenPayload.AccountId, chatId, content),
               cancellationToken
           );

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<bool>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<bool>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }

        [HttpGet("api/getChatsSettings"), Authorize]
        [SwaggerOperation("Получить настройки чатов")]
        [SwaggerResponse(200, Type = typeof(ChatSettingsBody))]
        [SwaggerResponse(403)]
        public async Task<IActionResult> GetChatsSettings(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            CancellationToken cancellationToken
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetChatsSettings(tokenPayload.AccountId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<ChatSettingsBody>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<ChatSettingsBody>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }
    }
}