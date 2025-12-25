using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using CaesarServerLibrary.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace planner_node_service.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly INotificationService _notifyService;
        private readonly IServiceScopeFactory _scopeFactory;
        private ILogger<RabbitMqService> _logger;
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _messageSentToChatQueue;
        private readonly string _createPersonalChatQueue;

        private readonly string _messageSentToChatExchange;
        private readonly string _createPersonalChatExchange;

        public RabbitMqService(
            INotificationService notifyService,
            ILogger<RabbitMqService> logger,
            IServiceScopeFactory scopeFactory,
            string hostname,
            string userName,
            string password,
            string queue,
            string createPersonalChatQueue)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;
            _logger = logger;

            _scopeFactory = scopeFactory;

            _notifyService = notifyService;
            _messageSentToChatExchange = queue;
            _createPersonalChatExchange = createPersonalChatQueue;

            _messageSentToChatQueue = queue + "_node";
            _createPersonalChatQueue = createPersonalChatQueue + "_node";

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

            DeclareQueue(_messageSentToChatQueue, _messageSentToChatExchange);
            DeclareQueue(_createPersonalChatQueue, _createPersonalChatExchange);
        }

        private void DeclareQueue(string queueName, string exchange)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: queueName,
                exchange: exchange,
                routingKey: "");
        }

        private void ConsumeQueue(string queueName, Func<string, Task> handler)
        {
            _channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    await handler(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            ConsumeQueue(_messageSentToChatQueue, HandleSendMessage);
            ConsumeQueue(_createPersonalChatQueue, HandleNewChat);

            await Task.CompletedTask;
        }

        private async Task HandleSendMessage(string message)
        {
            var result = JsonSerializer.Deserialize<MessageSentToChatEvent>(message);

            _logger.LogInformation($"NodeService received message: {JsonSerializer.Deserialize<ChatMessageInfo>(result.Message)}");

            if (result == null)
                return;

            var chatMessage = JsonSerializer.Deserialize<ChatMessageInfo>(result.Message);

            using var scope = _scopeFactory.CreateScope();
            var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();

            await nodeService.AddOrUpdateNode(new Node()
            {
                Id = chatMessage.Message.Id,
                Name = "Message",
                Type = NodeType.Chat,
                BodyJson = JsonSerializer.Serialize(chatMessage.Message)
            });

            await nodeService.AddOrUpdateNodeLink(new CreateOrUpdateNodeLink()
            {
                Id = chatMessage.Message.Id,
                ParentId = chatMessage.ChatId,
                ChildId = chatMessage.Message.Id,
                RelationType = RelationType.Contains
            });

            foreach (var accountId in result.AccountIds)
                await _notifyService.SendMessageToSessions(accountId, result.Message);

            foreach (var accountSession in result.AccountSessions)
                await NotifySessions(result.Message, accountSession);
        }

        private async Task HandleNewChat(string message)
        {
            var result = JsonSerializer.Deserialize<CreatePersonalChatEvent>(message);

            _logger.LogInformation($"NodeService received new chat: {message}");

            if (result == null)
                return;

            try
            {
                _logger.LogInformation($"{result.Chat}");

                using var scope = _scopeFactory.CreateScope();
                var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
                var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();

                await nodeService.AddOrUpdateNode(new Node()
                {
                    Id = result.Chat.Id,
                    Name = result.Chat.Name,
                    Type = NodeType.Chat,
                    BodyJson = JsonSerializer.Serialize(result.Chat)
                });

                foreach (var participant in result.Participants)
                {
                    await accessService.CreateAccessRight(participant.AccountId, result.Chat.Id, AccessType.Admin);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding chat node");
                throw;
            }
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