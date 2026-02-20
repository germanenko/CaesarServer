using Microsoft.Extensions.Logging;
using planner_chat_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Events.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace planner_chat_service.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly ILogger<RabbitMqNotifyService> _logger;

        private readonly string _createChatQueueName;
        private readonly string _messageSentToChatQueueName;
        private readonly string _createPersonalChatQueueName;
        private readonly string _getNotificationSettings;
        private readonly string _checkAccess;

        private readonly List<string> _exchanges = new List<string>();

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string createChatQueueName,
            string messageSentToChatQueueName,
            string createPersonalChatQueueName,
            string getUsersWithEnabledNotifications,
            string checkAccess,
            ILogger<RabbitMqNotifyService> logger)
        {
            _logger = logger;

            _hostname = hostname;
            _username = username;
            _password = password;
            _createChatQueueName = createChatQueueName;
            _messageSentToChatQueueName = messageSentToChatQueueName;
            _createPersonalChatQueueName = createPersonalChatQueueName;
            _getNotificationSettings = getUsersWithEnabledNotifications;
            _checkAccess = checkAccess;

            _exchanges.AddRange(new[] { _createChatQueueName, _messageSentToChatQueueName, _createPersonalChatQueueName, _getNotificationSettings, _checkAccess });

            ExchangeDeclare();
        }

        public void ExchangeDeclare()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            foreach (var exchange in _exchanges)
            {
                channel.ExchangeDeclare(
                    exchange: exchange,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    arguments: null);
            }
        }

        public async Task<ServiceResponse<TResult>> Publish<T, TResult>(T message, PublishEvent eventType)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var queue = GetQueueName(eventType);

            channel.ExchangeDeclare(exchange: queue,
                                 type: ExchangeType.Fanout,
                                 durable: true,
                                 autoDelete: false,
                                 arguments: null);

            var replyQueueName = channel.QueueDeclare(queue: "", exclusive: true, autoDelete: true).QueueName;

            var consumer = new EventingBasicConsumer(channel);

            var tcs = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var correlationId = Guid.NewGuid().ToString();

            var consumerTag = channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true
            );

            var complete = new ServiceResponse<TResult>();

            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var response = Encoding.UTF8.GetString(ea.Body.ToArray());

                    _logger.LogInformation($"Received response: {response}");

                    complete = JsonSerializer.Deserialize<ServiceResponse<TResult>>(response);

                    tcs.TrySetResult(response);
                    channel.BasicCancel(consumerTag);
                }
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: queue,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);

            var timeout = Task.Delay(TimeSpan.FromSeconds(10));
            var completedTask = await Task.WhenAny(tcs.Task, timeout);

            if (completedTask == timeout)
            {
                _logger.LogInformation("Timeout! No response received.");
                return new ServiceResponse<TResult>()
                {
                    IsSuccess = false,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            var response = await tcs.Task;

            _logger.LogInformation($"RabbitMQ Response : {response}");

            return complete;
        }

        public void Publish<T>(T message, PublishEvent eventType)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var queue = GetQueueName(eventType);

            channel.ExchangeDeclare(exchange: queue,
                                 type: ExchangeType.Fanout,
                                 durable: true,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(exchange: queue,
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);
        }

        private string GetQueueName(PublishEvent eventType)
        {
            return eventType switch
            {
                PublishEvent.AddAccountToChat => _createChatQueueName,
                PublishEvent.MessageSentToChat => _messageSentToChatQueueName,
                PublishEvent.CreatePersonalChat => _createPersonalChatQueueName,
                PublishEvent.GetNotificationSettings => _getNotificationSettings,
                PublishEvent.CheckAccess => _checkAccess,
                _ => throw new ArgumentException("Invalid event type")
            };
        }
    }
}