using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planner_Auth.Core.Entities.Events;
using Planner_Auth.Core.Entities.Request;
using Planner_Auth.Core.IRepository;
using Planner_Auth.Core.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static Planner_Auth.Core.Entities.Events.CreateChatEvent;

namespace Planner_Auth.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private readonly INotifyService _notifyService;
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly string _hostname;
        private readonly string _updateProfileQueue;
        private readonly string _queueCreateChatName;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string userName,
            string password,
            string updateProfileQueue,
            string queueCreateChatName,
            INotifyService notifyService)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;
            _serviceFactory = serviceFactory;
            _notifyService = notifyService;

            _updateProfileQueue = updateProfileQueue;
            _queueCreateChatName = queueCreateChatName;

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

            DeclareQueue(_updateProfileQueue);
            DeclareQueue(_queueCreateChatName);
        }

        private void DeclareQueue(string queueName)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
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

            ConsumeQueue(_updateProfileQueue, HandleUpdateProfileMessageAsync);
            ConsumeQueue(_queueCreateChatName, HandleCreateChatMessageAsync);

            await Task.CompletedTask;
        }

        private async Task HandleUpdateProfileMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var updateProfileBody = JsonSerializer.Deserialize<UpdateProfileBody>(message);
            if (updateProfileBody == null)
                return;

            var account = await accountRepository.UpdateProfileIconAsync(updateProfileBody.AccountId, updateProfileBody.FileName);
        }

        private async Task HandleCreateChatMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var createChatBody = JsonSerializer.Deserialize<CreateChatResponseEvent>(message);
            if (createChatBody == null)
                return;

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

            var createChatEvent = new CreateChatEvent
            {
                ChatId = createChatBody.ChatId,
                Participants = result
            };

            _notifyService.Publish(createChatEvent, PublishEvent.InitChat);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}