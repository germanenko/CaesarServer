using System.Text;
using System.Text.Json;
using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Events;
using CaesarServerLibrary.Enums;
using Microsoft.Extensions.Hosting;
using planner_node_service.Core.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly INotificationService _notifyService;
        private readonly INodeService _nodeService;
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _queue;
        private readonly string _createPersonalChatQueue;

        public RabbitMqService(
            INotificationService notifyService,
            string hostname,
            string userName,
            string password,
            string queue,
            string createPersonalChatQueue)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;

            _notifyService = notifyService;
            _queue = queue;
            _createPersonalChatQueue = createPersonalChatQueue;

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

            DeclareQueue(_queue);
            DeclareQueue(_createPersonalChatQueue);
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
            ConsumeQueue(_queue, HandleSendMessage);
            ConsumeQueue(_createPersonalChatQueue, HandleNewChat);

            await Task.CompletedTask;
        }

        private async Task HandleSendMessage(string message)
        {
            var result = JsonSerializer.Deserialize<MessageSentToChatEvent>(message);
            if (result == null)
                return;

            foreach (var accountId in result.AccountIds)
                await _notifyService.SendMessageToSessions(accountId, result.Message);

            foreach (var accountSession in result.AccountSessions)
                await NotifySessions(result.Message, accountSession);

            var chatMessage = JsonSerializer.Deserialize<ChatMessageInfo>(result.Message);

            await _nodeService.AddOrUpdateNode(new Node()
            {
                Id = chatMessage.Message.Id,
                Name = "Message",
                Type = NodeType.Chat,
                BodyJson = JsonSerializer.Serialize(chatMessage.Message)
            });

            await _nodeService.AddOrUpdateNodeLink(new CreateOrUpdateNodeLink()
            {
                Id = chatMessage.Message.Id,
                ParentId = chatMessage.ChatId,
                ChildId = chatMessage.Message.Id,
                RelationType = RelationType.Contains
            });
        }

        private async Task HandleNewChat(string message)
        {
            var result = JsonSerializer.Deserialize<CreatePersonalChatEvent>(message);
            if (result == null)
                return;

            await _nodeService.AddOrUpdateNode(new Node()
            {
                Id = result.Chat.Id,
                Name = result.Chat.Name,
                Props = JsonSerializer.Serialize(result.Chat),
                Type = NodeType.Chat,
                BodyJson = JsonSerializer.Serialize(result.Chat)
            });
        }

        private async Task<AccountSessions?> NotifySessions(byte[] bytes, AccountSessions accountSessions)
        {
            var sessionsNotReceiveMessage = await _notifyService.SendMessageToSessions(accountSessions.AccountId, accountSessions.SessionIds.ToList(), bytes);
            return sessionsNotReceiveMessage.Any() ? new AccountSessions
            {
                AccountId = accountSessions.AccountId,
                SessionIds = sessionsNotReceiveMessage.ToList()
            } : null;
        }
    }
}