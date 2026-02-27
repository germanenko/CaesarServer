using Microsoft.Extensions.Hosting;
using planner_common_package.Enums;
using planner_file_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace planner_file_service.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly INotifyService _notifyService;
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;

        private readonly Dictionary<string, (string QueueName, Func<string, Task> Handler)> _queues = new Dictionary<string, (string QueueName, Func<string, Task> Handler)>();

        public RabbitMqService(
            INotifyService notifyService,
            string hostname,
            string userName,
            string password)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;

            _notifyService = notifyService;


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

            foreach (var queue in _queues)
            {
                DeclareQueue(queue.Key, queue.Value.QueueName);
            }
        }

        private void DeclareQueue(string exchange, string queue)
        {
            _channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: queue,
                exchange: exchange,
                routingKey: "");
        }

        private void ConsumeQueue(string queueName, Func<string, Task> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await handler(message);
            };
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
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

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}