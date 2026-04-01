using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_chat_service.Core.IRepository;
using planner_chat_service.Core.IService;
using planner_common_package.Enums;
using planner_server_package;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace planner_chat_service.Infrastructure.Service
{
    public class RabbitMqService : RabbitMQServiceBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPublisherService _publisherService;
        private readonly IChatConnectionService _chatConnectionService;

        public RabbitMqService(
            IServiceScopeFactory scopeFactory,
            string hostname,
            string userName,
            string password,
            string prefix,
            IPublisherService publisherService,
            ILogger<RabbitMQServiceBase> logger,
            string chatAddAccountsToTaskChats,
            string chatNodesExchange)
            : base(hostname, userName, password, prefix, logger)
        {
            _scopeFactory = scopeFactory;

            _publisherService = publisherService;

            AddQueue(chatAddAccountsToTaskChats, HandleAddAccountToTaskChatMessageAsync);
            AddQueue(chatNodesExchange, HandleChatNodes);

            InitializeRabbitMQ();
        }


        private async Task<ServiceResponse<object>> HandleAddAccountToTaskChatMessageAsync(string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();

            var addAccountToTaskChatBody = JsonSerializer.Deserialize<AddAccountsToTaskChatsEvent>(message);
            if (addAccountToTaskChatBody == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            if (addAccountToTaskChatBody.AccountIds.Count == 0 || addAccountToTaskChatBody.TaskIds.Count == 0)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Нет аккаунтов или задач" }
                };

            foreach (var taskId in addAccountToTaskChatBody.TaskIds)
                await chatRepository.CreateChatSettingsAsync(taskId, addAccountToTaskChatBody.AccountIds);

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        private async Task<ServiceResponse<object>> HandleChatNodes(string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
            var nodes = JsonSerializer.Deserialize<SyncEntitiesEvent>(message);
            if (nodes == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Нет аккаунтов или задач" }
                };

            var chats = nodes.Bodies.OfType<ChatBody>().ToList();
            var messages = nodes.Bodies.OfType<MessageBody>().ToList();

            foreach (var chat in chats)
            {
                await chatService.CreatePersonalChat(nodes.TokenPayload.AccountId, nodes.TokenPayload.SessionId, new planner_client_package.Entities.CreateChatBody() { Id = chat.Id, CompanionId = chat.ParticipantIds.FirstOrDefault(x => x != nodes.TokenPayload.AccountId) });
            }

            foreach (var chatMessage in messages)
            {
                await chatService.SendMessageToChat(chatMessage.SenderId, chatMessage.SenderDeviceId, chatMessage.Link.ParentId, chatMessage.Content);
            }

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }


        //public async Task SendMessage(
        //    IEnumerable<ChatSession> sessions,
        //    MessageBody message,
        //    WebSocketMessageType messageType,
        //    IEnumerable<Guid> userIds,
        //    ChatType chatType,
        //    Guid chatId
        //)
        //{
        //    var chatMessageBody = new ChatMessageInfo
        //    {
        //        ChatId = chatId,
        //        ChatType = chatType,
        //        Message = message
        //    };

        //    var connectedSessionIds = sessions.Select(e => e.SessionId);
        //    var connectedAccountIds = sessions.GroupBy(e => e.AccountId).Select(e => e.Key);
        //    var notConnectedAccountIds = userIds.Except(connectedAccountIds);

        //    var str = JsonSerializer.Serialize(chatMessageBody);
        //    var bytes = Encoding.UTF8.GetBytes(str);
        //    var userSessionsDeliveryMessage = await SendMessageToConnectedUsers(sessions, bytes, messageType);
        //    DeliverMessageToDisconnectedUsers(notConnectedAccountIds, userSessionsDeliveryMessage, bytes);
        //}

        //private async Task<IEnumerable<AccountSessions>> SendMessageToConnectedUsers(IEnumerable<ChatSession> sessions, byte[] bytes, WebSocketMessageType messageType)
        //{
        //    var userSessionsDeliveryMessage = new List<AccountSessions>();

        //    foreach (var groupedSessions in sessions.GroupBy(e => e.AccountId))
        //    {
        //        var sessionsReceivedMessage = new List<Guid>();
        //        foreach (var session in groupedSessions)
        //        {
        //            if (await SendMessageToSession(session.Ws, bytes, messageType))
        //                sessionsReceivedMessage.Add(session.SessionId);
        //        }

        //        if (sessionsReceivedMessage.Any())
        //            userSessionsDeliveryMessage.Add(new AccountSessions { AccountId = groupedSessions.Key, SessionIds = sessionsReceivedMessage });
        //    }

        //    return userSessionsDeliveryMessage;
        //}

        //private async Task<bool> SendMessageToSession(WebSocket webSocket, byte[] bytes, WebSocketMessageType messageType)
        //{
        //    try
        //    {
        //        await webSocket.SendAsync(bytes, messageType, true, CancellationToken.None);
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //private void DeliverMessageToDisconnectedUsers(IEnumerable<Guid> accountIds, IEnumerable<AccountSessions> accountSessions, byte[] bytes)
        //{
        //    var messageSentToChatEvent = new MessageSentToChatEvent
        //    {
        //        AccountIds = accountIds,
        //        AccountSessions = accountSessions,
        //        Message = bytes
        //    };

        //    _notifyService.Publish(messageSentToChatEvent, PublishEvent.MessageSentToChat);
        //}
    }
}