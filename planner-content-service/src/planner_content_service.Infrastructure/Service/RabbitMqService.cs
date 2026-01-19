using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace planner_content_service.Infrastructure.Service
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

        private readonly Dictionary<string, (string QueueName, Func<string, Task> Handler)> _queues;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            INotifyService notifyService,
            string hostname,
            string userName,
            string password,
            string createTaskChatResponseQueue,
            string contentNodes)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;

            _notifyService = notifyService;

            _queues = new Dictionary<string, (string QueueName, Func<string, Task> Handler)>
            {
                { createTaskChatResponseQueue, (QueueName: GetQueueName(createTaskChatResponseQueue), Handler: HandleCreateTaskChatResponseMessageAsync) },
                { contentNodes, (QueueName: GetQueueName(contentNodes), Handler: HandleContentNodes) }
            };

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

            //ConsumeQueue(_createTaskChatResponseQueue, HandleCreateTaskChatResponseMessageAsync);
            await Task.CompletedTask;
        }

        private async Task HandleContentNodes(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();
            var response = JsonSerializer.Deserialize<SyncEntitiesEvent>(message);
            if (response == null)
                return;

            var boards = response.Bodies.OfType<BoardBody>().ToList();
            var columns = response.Bodies.OfType<ColumnBody>().ToList();
            var tasks = response.Bodies.OfType<TaskBody>().ToList();

            await boardService.CreateBoardsAsync(boards, response.TokenPayload.AccountId);
            await boardService.AddColumns(response.TokenPayload.AccountId, columns);
            await taskService.CreateOrUpdateTasks(response.TokenPayload.AccountId, tasks);
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

        private string GetQueueName(string exchange)
        {
            return exchange + "_content";
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}