using Microsoft.Extensions.Logging;
using planner_common_package.Enums;
using planner_node_service.Core.IService;
using planner_server_package.Events.Enums;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace planner_node_service.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly string _contentNodesExchange;
        private readonly string _chatNodesExchange;

        private readonly List<string> _exchanges = new List<string>();

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string contentNodesExchange,
            string chatNodesExchange)
        {

            _hostname = hostname;
            _username = username;
            _password = password;

            _contentNodesExchange = contentNodesExchange;
            _chatNodesExchange = chatNodesExchange;

            _exchanges.AddRange(new[] { _contentNodesExchange, _chatNodesExchange });


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
                PublishEvent.ContentNodes => _contentNodesExchange,
                PublishEvent.ChatNodes => _chatNodesExchange,
                _ => throw new ArgumentException("Invalid publish event")
            };
        }
    }
}