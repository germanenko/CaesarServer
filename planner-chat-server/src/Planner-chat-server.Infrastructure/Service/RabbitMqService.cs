using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Planner_chat_server.Core.Entities.Events;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;
using Planner_chat_server.Core.IRepository;
using Planner_chat_server.Core.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Planner_chat_server.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly INotifyService _notifyService;
        private readonly IChatConnectionService _chatConnectionService;
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;

        private readonly string _queueInitChatName;
        private readonly string _chatAttachmentQueue;
        private readonly string _chatImageQueue;
        private readonly string _chatAddAccountsToTaskChats;
        private readonly string _createTaskChatQueue;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            INotifyService notifyService,
            IChatConnectionService chatConnectionService,
            string hostname,
            string userName,
            string password,
            string queueInitChatName,
            string chatAttachmentQueue,
            string chatImageQueue,
            string chatAddAccountsToTaskChats,
            string createTaskChatQueue)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;

            _queueInitChatName = queueInitChatName;
            _chatAttachmentQueue = chatAttachmentQueue;
            _chatImageQueue = chatImageQueue;
            _chatAddAccountsToTaskChats = chatAddAccountsToTaskChats;
            _createTaskChatQueue = createTaskChatQueue;

            _serviceFactory = serviceFactory;
            _notifyService = notifyService;
            _chatConnectionService = chatConnectionService;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _userName,
                Password = _password,
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            DeclareQueue(_queueInitChatName);
            DeclareQueue(_chatAttachmentQueue);
            DeclareQueue(_chatImageQueue);
            DeclareQueue(_chatAddAccountsToTaskChats);
            DeclareQueue(_createTaskChatQueue);
        }

        private void DeclareQueue(string queueName)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        private void ConsumeQueue(string queueName, Func<string, Task> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await handler(message);
            };
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            ConsumeQueue(_queueInitChatName, HandleInitChatMessageAsync);
            ConsumeQueue(_chatAddAccountsToTaskChats, HandleAddAccountToTaskChatMessageAsync);
            ConsumeQueue(_chatAttachmentQueue, HandleChatAttachmentMessageAsync);
            ConsumeQueue(_chatImageQueue, HandleChatImageMessageAsync);
            ConsumeQueue(_createTaskChatQueue, HandleCreateTaskChatMessageAsync);
            await Task.CompletedTask;
        }

        private async Task HandleCreateTaskChatMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();
            var result = JsonSerializer.Deserialize<CreateTaskChatEvent>(message);
            if (result == null || result.IsSuccess)
                return;

            var newChat = await chatRepository.CreateTaskChatAsync(result.CreateTaskChat.ChatName, result.CreateTaskChat.CreatorId, result.CreateTaskChat.TaskId);
            if (newChat == null)
                return;

            result.IsSuccess = true;
            result.CreateTaskChat.ChatId = newChat.Id;

            _notifyService.Publish(result, NotifyPublishEvent.ResponseTaskChat);
        }

        private async Task HandleAddAccountToTaskChatMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();

            var addAccountToTaskChatBody = JsonSerializer.Deserialize<AddAccountsToTaskChatsEvent>(message);
            if (addAccountToTaskChatBody == null)
                return;

            if (addAccountToTaskChatBody.AccountIds.Count == 0 || addAccountToTaskChatBody.TaskIds.Count == 0)
                return;

            foreach (var taskId in addAccountToTaskChatBody.TaskIds)
                await chatRepository.CreateChatMembershipsAsync(taskId, addAccountToTaskChatBody.AccountIds);
        }



        private async Task HandleInitChatMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();
            var createChatResponseEvent = JsonSerializer.Deserialize<CreateChatResponseEvent>(message);
            if (createChatResponseEvent == null)
                return;

            foreach (var participant in createChatResponseEvent.Participants)
            {
                var chatMembership = await chatRepository.GetMembershipAsync(createChatResponseEvent.ChatId, participant.AccountId);
                if (chatMembership == null)
                    continue;

                var date = DateTime.Now;
                await chatRepository.CreateAccountChatSessionAsync(participant.SessionIds, chatMembership, date);
            }
        }

        private async Task HandleChatAttachmentMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();
            var chatAttachment = JsonSerializer.Deserialize<ChatAttachmentEvent>(message);
            if (chatAttachment == null)
                return;

            var chat = await chatRepository.GetAsync(chatAttachment.ChatId);
            if (chat == null)
                return;

            var chatMessage = await chatRepository.AddMessageAsync(MessageType.File, chatAttachment.FileName, chat, chatAttachment.AccountId);
            if (chatMessage == null)
                return;

            var lobby = _chatConnectionService.GetConnections(chatAttachment.ChatId);
            if (lobby == null)
                return;

            var sessions = lobby.ActiveSessions.Select(e => e.Value);
            var account = lobby.AllChatUsers;

            await SendMessage(
                sessions,
                chatMessage.ToMessageBody(),
                WebSocketMessageType.Text,
                account,
                Enum.Parse<ChatType>(chat.Type),
                chatAttachment.ChatId);

        }

        private async Task HandleChatImageMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();
            var chatImage = JsonSerializer.Deserialize<ChatImageEvent>(message);
            if (chatImage == null)
                return;

            await chatRepository.UpdateChatImage(chatImage.ChatId, chatImage.Filename);
        }


        public async Task SendMessage(
            IEnumerable<ChatSession> sessions,
            MessageBody message,
            WebSocketMessageType messageType,
            IEnumerable<Guid> userIds,
            ChatType chatType,
            Guid chatId
        )
        {
            var chatMessageBody = new ChatMessageInfo
            {
                ChatId = chatId,
                ChatType = chatType,
                Message = message
            };

            var connectedSessionIds = sessions.Select(e => e.SessionId);
            var connectedAccountIds = sessions.GroupBy(e => e.AccountId).Select(e => e.Key);
            var notConnectedAccountIds = userIds.Except(connectedAccountIds);

            var str = JsonSerializer.Serialize(chatMessageBody);
            var bytes = Encoding.UTF8.GetBytes(str);
            var userSessionsDeliveryMessage = await SendMessageToConnectedUsers(sessions, bytes, messageType);
            DeliverMessageToDisconnectedUsers(notConnectedAccountIds, userSessionsDeliveryMessage, bytes);
        }

        private async Task<IEnumerable<AccountSessions>> SendMessageToConnectedUsers(IEnumerable<ChatSession> sessions, byte[] bytes, WebSocketMessageType messageType)
        {
            var userSessionsDeliveryMessage = new List<AccountSessions>();

            foreach (var groupedSessions in sessions.GroupBy(e => e.AccountId))
            {
                var sessionsReceivedMessage = new List<Guid>();
                foreach (var session in groupedSessions)
                {
                    if (await SendMessageToSession(session.Ws, bytes, messageType))
                        sessionsReceivedMessage.Add(session.SessionId);
                }

                if (sessionsReceivedMessage.Any())
                    userSessionsDeliveryMessage.Add(new AccountSessions { AccountId = groupedSessions.Key, SessionIds = sessionsReceivedMessage });
            }

            return userSessionsDeliveryMessage;
        }

        private async Task<bool> SendMessageToSession(WebSocket webSocket, byte[] bytes, WebSocketMessageType messageType)
        {
            try
            {
                await webSocket.SendAsync(bytes, messageType, true, CancellationToken.None);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void DeliverMessageToDisconnectedUsers(IEnumerable<Guid> accountIds, IEnumerable<AccountSessions> accountSessions, byte[] bytes)
        {
            var messageSentToChatEvent = new MessageSentToChatEvent
            {
                AccountIds = accountIds,
                AccountSessions = accountSessions,
                Message = bytes
            };

            _notifyService.Publish(messageSentToChatEvent, NotifyPublishEvent.MessageSentToChat);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}