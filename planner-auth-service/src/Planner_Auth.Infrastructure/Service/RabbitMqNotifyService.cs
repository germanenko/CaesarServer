using System.Text;
using System.Text.Json;
using Planner_Auth.Core.Entities.Events;
using Planner_Auth.Core.IService;
using RabbitMQ.Client;

namespace Planner_Auth.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly string _queueInitChatName;
        private readonly string _createAccountMailCredentialsQueue;

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string queueInitChatName,
            string createAccountMailCredentialsQueue)
        {
            _hostname = hostname;
            _username = username;
            _password = password;

            _queueInitChatName = queueInitChatName;
            _createAccountMailCredentialsQueue = createAccountMailCredentialsQueue;
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

            var queueName = GetQueueName(eventType);
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: body);
        }

        private string GetQueueName(PublishEvent eventType)
        {
            return eventType switch
            {
                PublishEvent.InitChat => _queueInitChatName,
                PublishEvent.CreateAccountMailCredentials => _createAccountMailCredentialsQueue,
                _ => throw new ArgumentException("Invalid event type")
            };
        }
    }
}