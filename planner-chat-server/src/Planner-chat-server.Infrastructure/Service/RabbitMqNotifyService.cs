using System.Text;
using System.Text.Json;
using Planner_chat_server.Core.Enums;
using Planner_chat_server.Core.IService;
using RabbitMQ.Client;

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

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string createChatQueueName,
            string createTaskChatResponseQueueName,
            string messageSentToChatQueueName)
        {
            _hostname = hostname;
            _username = username;
            _password = password;
            _createChatQueueName = createChatQueueName;
            _createTaskChatResponseQueueName = createTaskChatResponseQueueName;
            _messageSentToChatQueueName = messageSentToChatQueueName;
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
            channel.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: queue,
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
                _ => throw new ArgumentException("Invalid event type")
            };
        }
    }
}