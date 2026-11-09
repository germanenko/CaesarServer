using FirebaseAdmin.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using planner_client_package.Entities.WebSockets;
using planner_notify_service.Core.IService;
using planner_server_package;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace planner_notify_service.Infrastructure.Service
{
    public class RabbitMqService : RabbitMQServiceBase
    {
        private readonly INotificationService _notificationService;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string userName,
            string password,
            string prefix,
            ILogger<RabbitMQServiceBase> logger,
            INotificationService notificationService,
            string messageSentToChatExchange,
            string sendNotificationExchange,
            string scopeUpdatedExchange,
            string accessRevokedExchange,
            string chatChangedExchange)
            : base(hostname, userName, password, prefix, logger)
        {
            _scopeFactory = serviceFactory;

            _notificationService = notificationService;

            AddQueue(messageSentToChatExchange, HandleSendMessage);
            AddQueue(sendNotificationExchange, SendNotification);
            AddQueue(scopeUpdatedExchange, HandleScopeUpdated);
            AddQueue(accessRevokedExchange, HandleAccessRevokedUpdated);
            AddQueue(chatChangedExchange, HandleChatChanged);

            InitializeRabbitMQ();
        }

        private async Task<ServiceResponse<object>> HandleSendMessage(string message)
        {
            var result = JsonSerializer.Deserialize<MessageSentToChatEvent>(message);
            if (result == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };


            var wsMessage = new WebSocketMessage() { MessageKind = planner_common_package.Enums.WebSocketMessageKind.ChatMessage, MessageJson = message };
            var wsMessageBytes = SerializeObject(wsMessage);

            foreach (var accountId in result.AccountIds)
                await _notificationService.SendMessageToSessions(accountId, wsMessageBytes);

            foreach (var accountSession in result.AccountSessions)
                await NotifySessions(wsMessageBytes, accountSession);

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        private async Task<ServiceResponse<object>> SendNotification(string message)
        {
            var result = JsonSerializer.Deserialize<NotificationBody>(message);
            if (result == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            using var scope = _scopeFactory.CreateScope();
            var notifyService = scope.ServiceProvider.GetRequiredService<INotifyService>();

            await notifyService.SendFCMNotification(result.AccountId, result.Title, result.Content, result.Data);

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        private async Task<ServiceResponse<object>> HandleScopeUpdated(string message)
        {
            var result = JsonSerializer.Deserialize<ScopeUpdatedEvent>(message);
            if (result == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            _logger.LogInformation($" === ScopeUpdated: {message} === ");

            var scopeUpdatedMessage = new ScopeUpdatedMessage()
            {
                ScopeId = result.ScopeId
            };

            WebSocketMessage wsMessage = new WebSocketMessage()
            {
                MessageKind = planner_common_package.Enums.WebSocketMessageKind.ScopeUpdated,
                MessageJson = JsonSerializer.Serialize(scopeUpdatedMessage)
            };

            foreach (var accountId in result.AccountIds)
                await _notificationService.SendMessageToSessions(accountId, SerializeObject(wsMessage));

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        private async Task<ServiceResponse<object>> HandleAccessRevokedUpdated(string message)
        {
            var result = JsonSerializer.Deserialize<AccessRevokedEvent>(message);
            if (result == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            _logger.LogInformation($" === AccessRevoked: {message} === ");

            var accessRevokedMessage = new AccessRevokedMessage()
            {
                NodeId = result.NodeId
            };

            WebSocketMessage wsMessage = new WebSocketMessage()
            {
                MessageKind = planner_common_package.Enums.WebSocketMessageKind.AccessRevoked,
                MessageJson = JsonSerializer.Serialize(accessRevokedMessage)
            };

            await _notificationService.SendMessageToSessions(result.AccountId, SerializeObject(wsMessage));

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }


        private async Task<ServiceResponse<object>> HandleChatChanged(string message)
        {
            var result = JsonSerializer.Deserialize<planner_client_package.Entities.ChatChangedEvent>(message);
            if (result == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            using var scope = _scopeFactory.CreateScope();
            var notifyService = scope.ServiceProvider.GetRequiredService<INotifyService>();

            var wsMessage = new WebSocketMessage()
            {
                MessageKind = planner_common_package.Enums.WebSocketMessageKind.ChatUpdated,
                MessageJson = JsonSerializer.Serialize(new ChatUpdatedMessage() { ChatId = result.ChatId })
            };

            foreach (var userId in result.Members)
                await _notificationService.SendMessageToSessions(userId, SerializeObject(wsMessage));

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }


        private async Task<AccountSessions?> NotifySessions(byte[] bytes, AccountSessions accountSessions)
        {
            var sessionsNotReceiveMessage = await _notificationService.SendMessageToSessions(accountSessions.AccountId, accountSessions.SessionIds.ToList(), bytes);
            return sessionsNotReceiveMessage.Any() ? new AccountSessions
            {
                AccountId = accountSessions.AccountId,
                SessionIds = sessionsNotReceiveMessage.ToList()
            } : null;
        }

        private byte[] SerializeObject<T>(T obj)
        {
            var serializableString = JsonSerializer.Serialize(obj, options);
            var bytes = Encoding.UTF8.GetBytes(serializableString);
            return bytes;
        }
    }
}