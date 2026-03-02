using Microsoft.Extensions.Logging;
using planner_server_package.Entities;
using planner_server_package.Events.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace planner_server_package.RabbitMQ
{
    public class RabbitMQPublisher : IPublisherService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly ILogger<RabbitMQPublisher> _logger;

        private readonly Dictionary<PublishEvent, string> _exchanges = new Dictionary<PublishEvent, string>();

        public RabbitMQPublisher(
            string hostname,
            string username,
            string password,
            Dictionary<PublishEvent, string> exchanges,
            ILogger<RabbitMQPublisher> logger)
        {
            _logger = logger;

            _hostname = hostname;
            _username = username;
            _password = password;

            _exchanges = exchanges;

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
                    exchange: exchange.Value,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    arguments: null);
            }
        }

        public async Task<ServiceResponse<object>> Publish<T>(T message, PublishEvent publishEvent)
        {
            var exchangeName = GetExchangeName(publishEvent);

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

            var tcs = new TaskCompletionSource<ServiceResponse<object>>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var correlationId = Guid.NewGuid().ToString();

            var consumerTag = channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true
            );

            var response = new ServiceResponse<object>();

            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());

                    _logger.LogInformation($"Received response: {responseJson}");

                    try
                    {

                        response = JsonSerializer.Deserialize<ServiceResponse<object>>(responseJson);

                        tcs.TrySetResult(response ?? new ServiceResponse<object>
                        {
                            IsSuccess = false,
                            Errors = new[] { "Invalid response format" }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error in response: " + ex.Message);
                    }

                    channel.BasicCancel(consumerTag);
                }
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = replyQueueName;
            properties.Persistent = true;

            _logger.LogInformation($"Publish event: {exchangeName}:{body}");

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);


            var timeout = Task.Delay(TimeSpan.FromSeconds(10));
            var completedTask = await Task.WhenAny(tcs.Task, timeout);

            if (completedTask == timeout)
            {
                channel.BasicCancel(consumerTag);
                _logger.LogInformation("Timeout! No response received.");
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            return await tcs.Task;
        }

        protected string GetExchangeName(PublishEvent eventType)
        {
            if (_exchanges.ContainsKey(eventType)) return _exchanges[eventType];
            else throw new ArgumentException("Invalid event type");
        }
    }
}
