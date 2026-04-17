using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IService;
using planner_server_package;
using planner_server_package.Converters;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.RabbitMQ;
using System.Text.Json;
using ClientNodeLinkBody = planner_client_package.Entities.NodeLinkBody;

namespace planner_node_service.Infrastructure.Service
{
    public class RabbitMqService : RabbitMQServiceBase
    {
        private readonly IWebSocketService _notificationService;
        private readonly IPublisherService _publisherService;
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string userName,
            string password,
            string prefix,
            IPublisherService publisherService,
            ILogger<RabbitMQServiceBase> logger,
            IWebSocketService notificationService,
            string queue,
            string createPersonalChatQueue,
            string createNode,
            string getUsersWithEnabledNotifications,
            string deleteNode)
            : base(hostname, userName, password, prefix, logger)
        {
            _scopeFactory = serviceFactory;

            _publisherService = publisherService;

            _notificationService = notificationService;

            AddQueue(queue, HandleSendMessage);
            AddQueue(createPersonalChatQueue, HandleNewChat);
            AddQueue(createNode, HandleNewNode);
            AddQueue(getUsersWithEnabledNotifications, HandleGetNotificationSettings);
            AddQueue(deleteNode, DeleteNode);

            InitializeRabbitMQ();
        }

        private async Task<ServiceResponse<object>> HandleSendMessage(string message)
        {
            var result = JsonSerializer.Deserialize<MessageSentToChatEvent>(message);

            _logger.LogInformation($"NodeService received message: {message}");

            if (result == null)
            {
                _logger.LogInformation($"Error while deserializing");
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };
            }

            var chatMessage = JsonSerializer.Deserialize<MessageBody>(result.Message);

            using var scope = _scopeFactory.CreateScope();
            var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
            var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();

            var can = (await accessService.CheckAccess(chatMessage.SenderId, chatMessage.Link.ParentId, Permission.Write)).Body;

            if (!can)
            {
                _logger.LogInformation($"Access denied for {chatMessage.SenderId} : {chatMessage.Link.ParentId}");

                return new ServiceResponse<object>()
                {
                    IsSuccess = can,
                    StatusCode = System.Net.HttpStatusCode.Forbidden,
                    Errors = new[] { "Нет доступа" }
                };
            }

            _logger.LogInformation($"Insert message: {message}");

            var chat = await nodeService.AddOrUpdateNode(BodyConverter.ServerToClientBody(chatMessage));

            foreach (var accountId in result.AccountIds)
                await _notificationService.SendMessageToSessions(accountId, result.Message);

            foreach (var accountSession in result.AccountSessions)
                await NotifySessions(result.Message, accountSession);

            if (chat.Body != null) _logger.LogInformation($"Message inserted completely");
            else _logger.LogInformation($"{JsonSerializer.Serialize(chat)}");

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
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                await nodeService.AddOrUpdateScope(BodyConverter.ServerToClientBody(result.Chat));

                foreach (var participant in result.ParticipantIds)
                {
                    await accessService.AddAccess(participant, result.Chat.Id, Permission.Write);
                    await notificationService.AddNotificationSettings(new NotificationSettingsBody() { AccountId = participant, NodeId = result.Chat.Id, NotificationsEnabled = true });
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



        private async Task<ServiceResponse<object>> HandleNewNode(string message)
        {
            var result = JsonSerializer.Deserialize<CreateNodeEvent>(message);

            _logger.LogInformation($"NodeService received new node: {message}");

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
                _logger.LogInformation($"{result.Node}");

                using var scope = _scopeFactory.CreateScope();
                var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();
                var accessService = scope.ServiceProvider.GetRequiredService<IAccessService>();

                if (result.Node.Link != null)
                {
                    var hasAccess = (await accessService.CheckAccess(result.CreatorId, result.Node.Link.ParentId, Permission.Write)).Body;

                    if (!hasAccess)
                    {
                        return new ServiceResponse<object>()
                        {
                            IsSuccess = false,
                            StatusCode = System.Net.HttpStatusCode.Forbidden,
                            Errors = new[] { "Нет доступа" }
                        };
                    }

                    await nodeService.AddOrUpdateNode(BodyConverter.ServerToClientBody(result.Node));
                }

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

                var settings = await notificationService.GetEnabledNotificationSettings(result.AccountIds);

                _logger.LogInformation($"Notification settings: {JsonSerializer.Serialize(settings.Body)}");

                return new ServiceResponse<object>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Body = settings.Body
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while return enabled notification settings");
                throw;
            }
        }

        private async Task<ServiceResponse<object>> DeleteNode(string message)
        {
            var result = JsonSerializer.Deserialize<DeleteNodeEvent>(message);

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
                var nodeService = scope.ServiceProvider.GetRequiredService<INodeService>();

                var delete = await nodeService.DeleteNode(result.AccountId, result.NodeId);

                return new ServiceResponse<object>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Body = delete.Body
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting node");
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
    }
}