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
    public class TaskChatController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IChatService _chatService;

        public TaskChatController(
            IJwtService jwtService,
            IChatService chatService
        )
        {
            _jwtService = jwtService;
            _chatService = chatService;
        }


        [HttpGet("taskChat"), Authorize]
        [SwaggerOperation("Подключиться к чату")]

        public async Task ConnectToTaskChat(
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

        [HttpPost("api/taskChat/messages")]
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


        [HttpGet("api/taskChats"), Authorize]
        [SwaggerOperation("Получить список чатов")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ChatBody>))]

        public async Task<IActionResult> GetChats(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _chatService.GetChats(tokenInfo.AccountId, tokenInfo.SessionId, ChatType.Task);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode);
        }

        // [HttpPost("api/taskChat"), Authorize]
        // [SwaggerOperation("Добавить пользователя к чату задачи")]
        // [SwaggerResponse(200, Description = "Возвращаются идентификаторы не добавленных пользователей", Type = typeof(IEnumerable<string>))]
        // [SwaggerResponse(400)]

        // public async Task<IActionResult> AddUsersToTaskChatMembership(
        //     [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
        //     [FromHeader, Required] List<Guid> ids,
        //     [FromHeader, Required] Guid chatId
        // )
        // {

        //     var accountIds = ids.ToHashSet().ToList();
        //     var users = await _userRepository.GetUsersAsync(accountIds);

        //     var chat = await _chatRepository.GetAsync(chatId);
        //     if (chat == null)
        //         return BadRequest("chatId is empty");

        //     if (users.Count != accountIds.Count)
        //         return BadRequest();

        //     var tokenPayload = _jwtService.GetTokenInfo(token);
        //     var taskCreator = await _userRepository.GetAsync(tokenPayload.UserId);
        //     if (taskCreator == null)
        //         return Unauthorized();

        //     var notAddedUsers = new List<string>();
        //     foreach (var user in users)
        //     {
        //         var userSessions = await _userRepository.GetUserSessionsAsync(user.Id);
        //         var result = await _chatRepository.AddMembershipAsync(user, chat);
        //         if (result == null)
        //             notAddedUsers.Add(user.Identifier);
        //         else
        //             await _chatRepository.CreateUserChatSessionAsync(userSessions, result, result.DateLastViewing);

        //         var message = new CreateMessageBody
        //         {
        //             Content = "Вы были выбраны исполнителем задачи",
        //             Type = MessageType.Text
        //         };

        //         var chatMessage = await _chatRepository.AddMessageAsync(message, chat, taskCreator);
        //         var messageBody = chatMessage.ToMessageBody();

        //         var bytes = SerializeObject(messageBody);
        //         await _notificationService.SendMessageToAllUserSessions(user.Id, bytes);
        //     }

        //     return Ok(notAddedUsers);
        // }
    }
}