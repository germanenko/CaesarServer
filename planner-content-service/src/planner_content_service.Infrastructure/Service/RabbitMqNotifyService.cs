using Microsoft.Extensions.Logging;
using planner_common_package.Enums;
using planner_content_service.Core.IService;
using planner_server_package.Events.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace planner_content_service.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly ILogger<RabbitMqNotifyService> _logger;

        private readonly string _createTaskChatResponseQueue;
        private readonly string _addAccountsToTaskChatsQueue;
        private readonly string _createBoardExchange;
        private readonly string _createColumnExchange;
        private readonly string _createTaskExchange;

        private readonly List<string> _exchanges = new List<string>();

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string createTaskChatResponseQueue,
            string addAccountsToTaskChatsQueue,
            string createBoardExchange,
            string createColumnExchange,
            string createTaskExchange,
            ILogger<RabbitMqNotifyService> logger)
        {

            _hostname = hostname;
            _username = username;
            _password = password;

            _createTaskChatResponseQueue = createTaskChatResponseQueue;
            _addAccountsToTaskChatsQueue = addAccountsToTaskChatsQueue;
            _createBoardExchange = createBoardExchange;
            _createColumnExchange = createColumnExchange;
            _createTaskExchange = createTaskExchange;

            _exchanges.AddRange(new[] { _createTaskChatResponseQueue, _addAccountsToTaskChatsQueue, _createBoardExchange, _createColumnExchange, _createTaskExchange });

            _logger = logger;

            ExchangeDeclare();
        }

        public void ExchangeDeclare()
        {
            Console.WriteLine("Start declare");

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

        public async Task<string> Publish<T>(T message, PublishEvent publishEvent)
        {
            var exchangeName = GetQueueName(publishEvent);

            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            _logger.LogInformation(JsonSerializer.Serialize(message));

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

            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation($"Received response: {response}");
                    tcs.TrySetResult(response);
                    channel.BasicCancel(consumerTag);
                }
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = replyQueueName;
            properties.Persistent = true;

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);

            var timeout = Task.Delay(TimeSpan.FromSeconds(10));
            var completedTask = await Task.WhenAny(tcs.Task, timeout);

            if (completedTask == timeout)
            {
                _logger.LogInformation("Timeout! No response received.");
                return "Timeout";
            }

            var response = await tcs.Task;

            _logger.LogInformation($"RabbitMQ Response : {response}");

            return response;
        }

        public string GetQueueName(PublishEvent publishEvent)
        {
            return publishEvent switch
            {
                PublishEvent.CreateTaskChatResponse => _createTaskChatResponseQueue,
                PublishEvent.AddAccountsToTaskChats => _addAccountsToTaskChatsQueue,
                PublishEvent.CreateBoard => _createBoardExchange,
                PublishEvent.CreateColumn => _createColumnExchange,
                PublishEvent.CreateTask => _createTaskExchange,
                _ => throw new ArgumentException("Invalid publish event")
            };
        }
    }
}