using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IService;
using planner_server_package.Converters;
using planner_server_package.Entities;
using planner_server_package.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace planner_node_service.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IWebSocketService _notificationService;
        private readonly INotifyService _notifyService;
        private readonly IServiceScopeFactory _scopeFactory;
        private ILogger<RabbitMqService> _logger;
        private readonly string _hostname;
        private readonly string _userName;
        private readonly string _password;

        private readonly Dictionary<string, (string QueueName, Func<string, Task<ServiceResponse<object>>> Handler)> _queues;

        public RabbitMqService(
            IWebSocketService notificationService,
            INotifyService notifyService,
            ILogger<RabbitMqService> logger,
            IServiceScopeFactory scopeFactory,
            string hostname,
            string userName,
            string password,
            string queue,
            string createPersonalChatQueue,
            string createBoard,
            string createColumn,
            string createTask,
            string getUsersWithEnabledNotifications,
            string checkAccess)
        {
            _hostname = hostname;
            _userName = userName;
            _password = password;
            _logger = logger;

            _scopeFactory = scopeFactory;
            _notifyService = notifyService;

            _notificationService = notificationService;

            _queues = new Dictionary<string, (string QueueName, Func<string, Task<ServiceResponse<object>>> Handler)>
            {
                { queue, (QueueName: GetQueueName(queue), Handler: HandleSendMessage) },
                { createPersonalChatQueue, (QueueName: GetQueueName(createPersonalChatQueue), Handler: HandleNewChat) },
                { createBoard, (QueueName: GetQueueName(createBoard), Handler: HandleNewBoard) },
                { createColumn, (QueueName: GetQueueName(createColumn), Handler: HandleNewColumn) },
                { createTask, (QueueName: GetQueueName(createTask), Handler: HandleNewTask) },
                { getUsersWithEnabledNotifications, (QueueName: GetQueueName(getUsersWithEnabledNotifications), Handler: HandleGetNotificationSettings) },
                { checkAccess, (QueueName: GetQueueName(checkAccess), Handler: CheckAccess) }
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

            //ConsumeQueue(_messageSentToChatQueue, HandleSendMessage);
            //ConsumeQueue(_createPersonalChatQueue, HandleNewChat);

            await Task.CompletedTask;
        }

        private async Task<ServiceResponse<object>> HandleSendMessage(string message)
        {
            var result = JsonSerializer.Deserialize<MessageSentToChatEvent>(message);

            _logger.LogInformation($"NodeService received message: {JsonSerializer.Deserialize<ChatMessageInfo>(result.Message)}");

            if (result == null)
            {
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            var chatMessage = JsonSerializer.Deserialize<ChatMessageInfo>(result.Message);

            using var scope = _scopeFactory.CreateScope();
            var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
            var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();

            var can = (await accessService.CheckAccess(chatMessage.Message.SenderId, chatMessage.ChatId)).Body;

            if (!can)
            {
                return new ServiceResponse<object>()
                {
                    IsSuccess = can,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            await nodeService.AddOrUpdateNode(new Node()
            {
                Id = chatMessage.Message.Id,
                Name = "Message",
                Type = NodeType.Message,
                BodyJson = JsonSerializer.Serialize<NodeBody>(chatMessage.Message)
            });

            await nodeService.AddOrUpdateNodeLink(BodyConverter.ServerToClientBody(chatMessage.Message.Link));

            foreach (var accountId in result.AccountIds)
                await _notificationService.SendMessageToSessions(accountId, result.Message);

            foreach (var accountSession in result.AccountSessions)
                await NotifySessions(result.Message, accountSession);

            return new ServiceResponse<object>()
            {
                IsSuccess = true
            };
        }

        private async Task<ServiceResponse<object>> HandleNewChat(string message)
        {
            var result = JsonSerializer.Deserialize<CreatePersonalChatEvent>(message);

            _logger.LogInformation($"NodeService received new chat: {message}");

            if (result == null)
            {
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            try
            {
                _logger.LogInformation($"{result.Chat}");

                using var scope = _scopeFactory.CreateScope();
                var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
                var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();

                await nodeService.AddOrUpdateNode(new Node()
                {
                    Id = result.Chat.Id,
                    Name = result.Chat.Name,
                    Type = NodeType.Chat,
                    BodyJson = JsonSerializer.Serialize<NodeBody>(result.Chat)
                });

                foreach (var participant in result.Participants)
                {
                    await accessService.CreateAccessRight(participant.AccountId, result.Chat.Id, Permission.Admin);
                }

                return new ServiceResponse<object>()
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding chat node");
                throw;
            }
        }

        private async Task<ServiceResponse<object>> HandleNewBoard(string message)
        {
            var result = JsonSerializer.Deserialize<CreateBoardEvent>(message);

            _logger.LogInformation($"NodeService received new board: {message}");

            if (result == null)
            {
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            try
            {
                _logger.LogInformation($"{result.Board}");

                using var scope = _scopeFactory.CreateScope();
                var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
                var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();
                var historyService = scope.ServiceProvider.GetRequiredService<IHistoryService>();

                await nodeService.AddOrUpdateNode(new Node()
                {
                    Id = result.Board.Id,
                    Name = result.Board.Name,
                    Type = NodeType.Board,
                    BodyJson = JsonSerializer.Serialize<NodeBody>(result.Board)
                });

                await accessService.CreateAccessRight(BodyConverter.ServerToClientBody(result.Board.AccessRight));

                await historyService.AddHistory(new History() { Id = Guid.NewGuid(), UpdatedById = result.CreatorId, Action = ActionType.Create, TrackableId = result.Board.Id, UpdatedAt = result.Board.UpdatedAt.Value });

                return new ServiceResponse<object>()
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding board node");
                throw;
            }
        }

        private async Task<ServiceResponse<object>> HandleNewColumn(string message)
        {
            var result = JsonSerializer.Deserialize<CreateColumnEvent>(message);

            _logger.LogInformation($"NodeService received new column: {message}");

            if (result == null)
            {
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            try
            {
                _logger.LogInformation($"{result.Column}");

                using var scope = _scopeFactory.CreateScope();
                var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
                var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();
                var historyService = scope.ServiceProvider.GetRequiredService<IHistoryService>();

                if (result.Column.Link != null)
                {
                    var can = (await accessService.CheckAccess(result.CreatorId, result.Column.Link.ParentId)).Body;

                    if (!can)
                    {
                        return new ServiceResponse<object>()
                        {
                            IsSuccess = false,
                            StatusCode = System.Net.HttpStatusCode.Forbidden,
                            Errors = new[] { "Нет доступа" }
                        };
                    }

                    await nodeService.AddOrUpdateNode(new Node()
                    {
                        Id = result.Column.Id,
                        Name = result.Column.Name,
                        Type = NodeType.Column,
                        BodyJson = JsonSerializer.Serialize<NodeBody>(result.Column)
                    });

                    await nodeService.AddOrUpdateNodeLink(BodyConverter.ServerToClientBody(result.Column.Link));
                }
                if (result.Column.AccessRight != null)
                {
                    await accessService.CreateAccessRight(BodyConverter.ServerToClientBody(result.Column.AccessRight));
                }

                await historyService.AddHistory(new History() { Id = Guid.NewGuid(), UpdatedById = result.CreatorId, Action = ActionType.Create, TrackableId = result.Column.Id, UpdatedAt = result.Column.UpdatedAt.Value });

                return new ServiceResponse<object>()
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding column node");
                throw;
            }
        }

        private async Task<ServiceResponse<object>> HandleNewTask(string message)
        {
            var result = JsonSerializer.Deserialize<CreateTaskEvent>(message);

            _logger.LogInformation($"NodeService received new task: {message}");

            if (result == null)
            {
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            try
            {
                _logger.LogInformation($"{result.Task}");

                using var scope = _scopeFactory.CreateScope();
                var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
                var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();
                var historyService = scope.ServiceProvider.GetRequiredService<IHistoryService>();

                await nodeService.AddOrUpdateNode(new Node()
                {
                    Id = result.Task.Id,
                    Name = result.Task.Name ?? "Task",
                    Type = NodeType.Task,
                    BodyJson = JsonSerializer.Serialize<NodeBody>(result.Task)
                });

                if (result.Task.Link != null)
                {
                    var can = (await accessService.CheckAccess(result.CreatorId, result.Task.Link.ParentId)).Body;

                    if (!can)
                    {
                        return new ServiceResponse<object>()
                        {
                            IsSuccess = false,
                            StatusCode = System.Net.HttpStatusCode.Forbidden,
                            Errors = new[] { "Нет доступа" }
                        };
                    }

                    await nodeService.AddOrUpdateNodeLink(BodyConverter.ServerToClientBody(result.Task.Link));
                }
                if (result.Task.AccessRight != null)
                {
                    await accessService.CreateAccessRight(BodyConverter.ServerToClientBody(result.Task.AccessRight));
                }

                await historyService.AddHistory(new History() { Id = Guid.NewGuid(), UpdatedById = result.CreatorId, Action = ActionType.Create, TrackableId = result.Task.Id, UpdatedAt = result.Task.UpdatedAt.Value });

                return new ServiceResponse<object>()
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding task node");
                throw;
            }
        }

        private async Task<ServiceResponse<object>> HandleGetNotificationSettings(string message)
        {
            var result = JsonSerializer.Deserialize<GetNotificationSettingsRequest>(message);

            _logger.LogInformation($"NodeService received request: {message}");

            if (result == null)
            {
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            try
            {
                _logger.LogInformation($"{result.AccountIds}");

                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var settings = notificationService.GetEnabledNotificationSettings(result.AccountIds);

                return new ServiceResponse<object>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Body = settings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while return enabled notification settings");
                throw;
            }
        }

        private async Task<ServiceResponse<object>> CheckAccess(string message)
        {
            var result = JsonSerializer.Deserialize<CheckAccessRequest>(message);

            _logger.LogInformation($"NodeService received request: {message}");

            if (result == null)
            {
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();

                var access = await accessService.CheckAccess(result.AccountId, result.NodeId);

                return new ServiceResponse<object>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Body = access
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while return enabled notification settings");
                throw;
            }
        }

        private async Task<AccountSessions?> NotifySessions(byte[] bytes, AccountSessions accountSessions)
        {
            var sessionsNotReceiveMessage = await _notificationService.SendMessageToSessions(accountSessions.AccountId, accountSessions.SessionIds.ToList(), bytes);
            return sessionsNotReceiveMessage.Any() ? new AccountSessions
            {
                AccountId = accountSessions.AccountId,
                SessionIds = sessionsNotReceiveMessage.ToList()
            } : null;
        }

        private string GetQueueName(string exchange)
        {
            return exchange + "_node";
        }
    }
}