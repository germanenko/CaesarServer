using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_auth_service.Core.IRepository;
using planner_auth_service.Core.IService;
using planner_common_package.Enums;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static planner_server_package.Events.CreateChatResponseEvent;

namespace planner_auth_service.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private readonly INotifyService _notifyService;
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;

        private readonly ILogger<RabbitMqService> _logger;

        private readonly Dictionary<string, (string QueueName, Func<string, Task<ServiceResponse<object>>> Handler)> _queues;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string userName,
            string password,
            string updateProfileQueue,
            string queueCreateChatName,
            string getGoogleTokenExchange,
            INotifyService notifyService,
            ILogger<RabbitMqService> logger)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;
            _serviceFactory = serviceFactory;
            _notifyService = notifyService;

            _logger = logger;

            _queues = new Dictionary<string, (string QueueName, Func<string, Task<ServiceResponse<object>>> Handler)>
            {
                { updateProfileQueue, (QueueName: GetQueueName(updateProfileQueue), Handler: HandleUpdateProfileMessageAsync) },
                { queueCreateChatName, (QueueName: GetQueueName(queueCreateChatName), Handler: HandleCreateChatMessageAsync) },
                { getGoogleTokenExchange, (QueueName: GetQueueName(getGoogleTokenExchange), Handler: HandleGoogleTokenRequest) }
            };

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

        private void DeclareQueue(string exchange, string queue)
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

        private void ConsumeQueue(string queueName, Func<string, Task<ServiceResponse<object>>> handler)
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

        private async Task<ServiceResponse<object>> HandleUpdateProfileMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var updateProfileBody = JsonSerializer.Deserialize<UpdateProfileBody>(message);
            if (updateProfileBody == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            var account = await accountRepository.UpdateProfileIconAsync(updateProfileBody.AccountId, updateProfileBody.FileName);

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        private async Task<ServiceResponse<object>> HandleCreateChatMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var createChatBody = JsonSerializer.Deserialize<CreateChatResponseEvent>(message);
            if (createChatBody == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            var result = new List<ChatParticipant>();

            foreach (var participant in createChatBody.Participants)
            {
                var sessions = await accountRepository.GetSessionsAsync(participant.AccountId);
                var chatMembership = new ChatParticipant
                {
                    AccountId = participant.AccountId,
                    ChatMembershipId = participant.ChatMembershipId,
                    SessionIds = sessions.Select(s => s.Id)
                };
                result.Add(chatMembership);
            }

            var createChatEvent = new CreateChatResponseEvent
            {
                ChatId = createChatBody.ChatId,
                Participants = result
            };

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        private async Task<ServiceResponse<object>> HandleGoogleTokenRequest(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var username = JsonSerializer.Deserialize<string>(message);
            if (string.IsNullOrEmpty(username))
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            var user = (await accountRepository.GetAccountsByPatternIdentifier(username)).FirstOrDefault();

            if (user == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Пользователя не существует" }
                };

            var googleToken = await accountRepository.GetGoogleTokenAsync(user.Id);

            if (googleToken == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Пользователь не добавил Google токен" }
                };

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Body = googleToken.ToBody()
            };
        }

        private string GetQueueName(string exchange)
        {
            return exchange + "_auth";
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}