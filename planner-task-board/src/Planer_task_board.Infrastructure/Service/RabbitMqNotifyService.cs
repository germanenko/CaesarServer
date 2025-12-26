using System.Text;
using System.Text.Json;
using CaesarServerLibrary.Enums;
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
            string createTaskExchange)
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
                channel.ExchangeDeclare(exchange: exchange,
                                     type: ExchangeType.Fanout,
                                     durable: true,
                                     autoDelete: false,
                                     arguments: null);
            }
        }

        public void Publish<T>(T message, PublishEvent publishEvent)
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
            channel.ExchangeDeclare(exchange: exchangeName,
                                 type: ExchangeType.Fanout,
                                 durable: true,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);
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