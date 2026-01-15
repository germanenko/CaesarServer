using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using CaesarServerLibrary.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.IRepository;
using Planner_chat_server.Core.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using static Google.Apis.Requests.BatchRequest;

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

        private readonly Dictionary<string, (string QueueName, Func<string, Task> Handler)> _queues;

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
            string chatNodesExchange)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;

            _queues = new Dictionary<string, (string QueueName, Func<string, Task> Handler)>
            {
                { queueInitChatName, (QueueName: GetQueueName(queueInitChatName), Handler: HandleInitChatMessageAsync) },
                { chatAttachmentQueue, (QueueName: GetQueueName(chatAttachmentQueue), Handler: HandleChatAttachmentMessageAsync) },
                { chatImageQueue, (QueueName: GetQueueName(chatImageQueue), Handler: HandleChatImageMessageAsync) },
                { chatAddAccountsToTaskChats, (QueueName: GetQueueName(chatAddAccountsToTaskChats), Handler: HandleAddAccountToTaskChatMessageAsync) },
                { chatNodesExchange, (QueueName: GetQueueName(chatNodesExchange), Handler: HandleChatNodes) },
            };

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

            foreach (var queue in _queues)
            {
                DeclareQueue(queue.Key, queue.Value.QueueName);
            }
        }

        private void DeclareQueue(string exchange, string queue)
        {
            _channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: queue,
                exchange: exchange,
                routingKey: "");
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

            foreach (var func in _queues)
            {
                ConsumeQueue(func.Value.QueueName, func.Value.Handler);
            }

            //ConsumeQueue(_queueInitChatName, HandleInitChatMessageAsync);
            //ConsumeQueue(_chatAddAccountsToTaskChats, HandleAddAccountToTaskChatMessageAsync);
            //ConsumeQueue(_chatAttachmentQueue, HandleChatAttachmentMessageAsync);
            //ConsumeQueue(_chatImageQueue, HandleChatImageMessageAsync);
            await Task.CompletedTask;
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
                await chatRepository.CreateChatSettingsAsync(taskId, addAccountToTaskChatBody.AccountIds);
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
                var chatMembership = await chatRepository.GetChatSettingsAsync(createChatResponseEvent.ChatId, participant.AccountId);
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

            var chatMessage = await chatRepository.AddMessageAsync(MessageType.File, chatAttachment.FileName, chat, chatAttachment.AccountId, Guid.NewGuid());
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
                chat.ChatType,
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

        private async Task HandleChatNodes(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
            var nodes = JsonSerializer.Deserialize<NodesEvent>(message);
            if (nodes == null)
                return;

            var chats = nodes.Nodes.OfType<ChatBody>().ToList();
            var messages = nodes.Nodes.OfType<MessageBody>().ToList();

            foreach (var chat in chats)
            {
                await chatService.CreatePersonalChat(nodes.TokenPayload.AccountId, nodes.TokenPayload.SessionId, chat, chat.ParticipantIds.FirstOrDefault(x => x != nodes.TokenPayload.AccountId));
            }

            foreach (var chatMessage in messages)
            {
                await chatService.SendMessageToChat(chatMessage.SenderId, chatMessage.ChatId, chatMessage.Content);
            }
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

        private string GetQueueName(string exchange)
        {
            return exchange + "_chat";
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}