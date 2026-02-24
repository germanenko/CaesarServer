using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_notify_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace planner_notify_service.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly INotificationService _notificationService;
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;

        private ILogger<RabbitMqService> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly Dictionary<string, (string QueueName, Func<string, Task<ServiceResponse<object>>> Handler)> _queues;

        public RabbitMqService(
            INotificationService notificationService,
            ILogger<RabbitMqService> logger,
            IServiceScopeFactory scopeFactory,
            string hostname,
            string userName,
            string password,
            string messageSentToChatExchange,
            string sendNotificationExchange)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;

            _logger = logger;
            _scopeFactory = scopeFactory;

            _notificationService = notificationService;

            _queues = new Dictionary<string, (string QueueName, Func<string, Task<ServiceResponse<object>>> Handler)>
            {
                { messageSentToChatExchange, (QueueName: GetQueueName(messageSentToChatExchange), Handler: HandleSendMessage) },
                { sendNotificationExchange, (QueueName: GetQueueName(sendNotificationExchange), Handler: SendNotification) }
            };

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

            _channel.ExchangeDeclare(
                exchange: "dlx-exchange",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null);

            _channel.QueueDeclare(
                queue: "dead-letter-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: "dead-letter-queue",
                exchange: "dlx-exchange",
                routingKey: "");

            foreach (var queue in _queues)
            {
                DeclareQueue(queue.Key, queue.Value.QueueName);
            }
        }

        private void DeclareQueue(string exchange, string queue)
        {
            var args = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "dlx-exchange" },
                { "x-dead-letter-routing-key", "" }
            };

            _channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: args);

            _channel.QueueBind(
                queue: queue,
                exchange: exchange,
                routingKey: "");
        }

        private void ConsumeQueue(string queueName, Func<string, Task<ServiceResponse<object>>> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"Received {queueName}: {message}");

                try
                {
                    var result = await handler(message);

                    if (!string.IsNullOrEmpty(ea.BasicProperties.ReplyTo))
                    {

                        var responseBody = Encoding.UTF8.GetBytes(
                            JsonSerializer.Serialize(result)
                        );

                        var replyProps = _channel.CreateBasicProperties();
                        replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                        _channel.BasicPublish(
                            exchange: "",
                            routingKey: ea.BasicProperties.ReplyTo,
                            basicProperties: replyProps,
                            body: responseBody
                        );
                    }


                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while receive message");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            foreach (var func in _queues)
            {
                ConsumeQueue(func.Value.QueueName, func.Value.Handler);
            }

            await Task.CompletedTask;
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

            foreach (var accountId in result.AccountIds)
                await _notificationService.SendMessageToSessions(accountId, result.Message);

            foreach (var accountSession in result.AccountSessions)
                await NotifySessions(result.Message, accountSession);

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

        private async Task<AccountSessions?> NotifySessions(byte[] bytes, AccountSessions accountSessions)
        {
            var sessionsNotReceiveMessage = await _notificationService.SendMessageToSessions(accountSessions.AccountId, accountSessions.SessionIds.ToList(), bytes);
            return sessionsNotReceiveMessage.Any() ? new AccountSessions
            {
                AccountId = accountSessions.AccountId,
                SessionIds = sessionsNotReceiveMessage.ToList()
            } : null;
        }

        private string GetQueueName(string exchange)
        {
            return exchange + "_notify";
        }
    }
}