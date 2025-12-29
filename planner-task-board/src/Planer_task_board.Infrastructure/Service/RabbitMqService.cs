using System.Text;
using System.Text.Json;
using CaesarServerLibrary.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Planer_task_board.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly INotifyService _notifyService;
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;

        private readonly string _createTaskChatResponseQueue;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            INotifyService notifyService,
            string hostname,
            string userName,
            string password,
            string createTaskChatResponseQueue)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;

            _notifyService = notifyService;

            _createTaskChatResponseQueue = createTaskChatResponseQueue;
            _serviceFactory = serviceFactory;

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

            DeclareQueue(_createTaskChatResponseQueue);
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
            ConsumeQueue(_createTaskChatResponseQueue, HandleCreateTaskChatResponseMessageAsync);
            await Task.CompletedTask;
        }

        private async Task HandleCreateTaskChatResponseMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var response = JsonSerializer.Deserialize<CreateTaskChatEvent>(message);
            if (response == null)
                return;

            await taskRepository.UpdateTaskChatId(response.CreateTaskChat.TaskId, (Guid)response.CreateTaskChat.ChatId);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}