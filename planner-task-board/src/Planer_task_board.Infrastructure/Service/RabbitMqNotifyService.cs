using System.Text;
using System.Text.Json;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IService;
using RabbitMQ.Client;

namespace Planer_task_board.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly string _createTaskChatResponseQueue;
        private readonly string _addAccountsToTaskChatsQueue;


        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string createTaskChatResponseQueue,
            string addAccountsToTaskChatsQueue)
        {
            _hostname = hostname;
            _username = username;
            _password = password;

            _createTaskChatResponseQueue = createTaskChatResponseQueue;
            _addAccountsToTaskChatsQueue = addAccountsToTaskChatsQueue;
        }

        public void Publish<T>(T message, PublishEvent publishEvent)
        {
            var queueName = GetQueueName(publishEvent);

            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
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

        public string GetQueueName(PublishEvent publishEvent)
        {
            return publishEvent switch
            {
                PublishEvent.CreateTaskChatResponse => _createTaskChatResponseQueue,
                PublishEvent.AddAccountsToTaskChats => _addAccountsToTaskChatsQueue,
                _ => throw new ArgumentException("Invalid publish event")
            };
        }
    }
}