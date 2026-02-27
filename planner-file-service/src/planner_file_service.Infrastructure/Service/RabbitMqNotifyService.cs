using Microsoft.Extensions.Logging;
using planner_file_service.Core.Enums;
using planner_file_service.Core.IService;
using planner_server_package.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace planner_file_service.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly string _profileImageQueue;
        private readonly string _chatImageQueue;
        private readonly string _chatAttachmentQueue;

        private readonly ILogger<RabbitMqNotifyService> _logger;

        private readonly List<string> _exchanges = new List<string>();

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string profileImageQueue,
            string chatImageQueue,
            string chatAttachmentQueue,
            ILogger<RabbitMqNotifyService> logger)
        {
            _hostname = hostname;
            _username = username;
            _password = password;

            _profileImageQueue = profileImageQueue;
            _chatImageQueue = chatImageQueue;
            _chatAttachmentQueue = chatAttachmentQueue;

            _logger = logger;

            _exchanges.AddRange(new[] { _profileImageQueue, _chatImageQueue, _chatAttachmentQueue });

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
                    exchange: exchange,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    arguments: null);
            }
        }

        public async Task<ServiceResponse<object>> Publish<T>(T message, ContentUploaded publishEvent)
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

        private string GetExchangeName(ContentUploaded eventType)
        {
            return eventType switch
            {
                ContentUploaded.ProfileImage => _profileImageQueue,
                ContentUploaded.ChatImage => _chatImageQueue,
                ContentUploaded.ChatAttachment => _chatAttachmentQueue,
                _ => throw new ArgumentException("Invalid event type")
            };
        }

        private string GetQueueName(ContentUploaded content)
        {
            return content switch
            {
                ContentUploaded.ProfileImage => _profileImageQueue,
                ContentUploaded.ChatImage => _chatImageQueue,
                ContentUploaded.ChatAttachment => _chatAttachmentQueue,
            };
        }
    }
}