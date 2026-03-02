using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_server_package.Entities;
using planner_server_package.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace planner_server_package.RabbitMQ
{
    public class RabbitMQServiceBase : BackgroundService
    {
        protected IConnection _connection;
        protected IModel _channel;
        protected readonly string _hostname;
        protected readonly string _userName;
        protected readonly string _password;

        protected readonly ILogger<RabbitMQServiceBase> _logger;

        protected readonly string _prefix;

        protected readonly Dictionary<string, (string QueueName, Func<string, Task<ServiceResponse<object>>> Handler)> _queues = new Dictionary<string, (string QueueName, Func<string, Task<ServiceResponse<object>>> Handler)>();

        public RabbitMQServiceBase(
            string hostname,
            string userName,
            string password,
            string prefix,
            ILogger<RabbitMQServiceBase> logger)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;

            _logger = logger;

            _prefix = prefix;
        }

        protected void InitializeRabbitMQ()
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

        protected void DeclareQueue(string exchange, string queue)
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

        protected void ConsumeQueue(string queueName, Func<string, Task<ServiceResponse<object>>> handler)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

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

        protected string GetQueueName(string exchange)
        {
            return exchange + _prefix;
        }

        protected void AddQueue(string queue, Func<string, Task<ServiceResponse<object>>> handler)
        {
            _queues.Add(queue, (QueueName: GetQueueName(queue), Handler: handler));
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
