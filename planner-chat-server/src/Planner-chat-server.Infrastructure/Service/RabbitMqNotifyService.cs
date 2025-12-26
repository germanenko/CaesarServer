using CaesarServerLibrary.Enums;
using Planner_chat_server.Core.IService;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Planner_chat_server.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly string _createChatQueueName;
        private readonly string _createTaskChatResponseQueueName;
        private readonly string _messageSentToChatQueueName;
        private readonly string _createPersonalChatQueueName;

        private readonly List<string> _exchanges = new List<string>();

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string createChatQueueName,
            string createTaskChatResponseQueueName,
            string messageSentToChatQueueName,
            string createPersonalChatQueueName)
        {
            _hostname = hostname;
            _username = username;
            _password = password;
            _createChatQueueName = createChatQueueName;
            _createTaskChatResponseQueueName = createTaskChatResponseQueueName;
            _messageSentToChatQueueName = messageSentToChatQueueName;
            _createPersonalChatQueueName = createPersonalChatQueueName;

            _exchanges.AddRange(new[] { _createChatQueueName, _createTaskChatResponseQueueName, _messageSentToChatQueueName, _createPersonalChatQueueName });

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

            foreach(var exchange in _exchanges)
            {
                try
                {
                    channel.ExchangeDeclarePassive(exchange);
                }
                catch (RabbitMQ.Client.Exceptions.OperationInterruptedException)
                {
                    channel.ExchangeDeclare(
                        exchange: exchange,
                        type: ExchangeType.Fanout,
                        durable: true,
                        autoDelete: false,
                        arguments: null);
                }
            }
        }

        public void Publish<T>(T message, NotifyPublishEvent eventType)
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

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: queue,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);
        }

        private string GetQueueName(NotifyPublishEvent eventType)
        {
            return eventType switch
            {
                NotifyPublishEvent.AddAccountToChat => _createChatQueueName,
                NotifyPublishEvent.ResponseTaskChat => _createTaskChatResponseQueueName,
                NotifyPublishEvent.MessageSentToChat => _messageSentToChatQueueName,
                NotifyPublishEvent.CreatePersonalChat => _createPersonalChatQueueName,
                _ => throw new ArgumentException("Invalid event type")
            };
        }
    }
}