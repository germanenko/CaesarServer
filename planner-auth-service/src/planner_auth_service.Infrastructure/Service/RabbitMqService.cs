using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_auth_service.Core.IRepository;
using planner_auth_service.Core.IService;
using planner_common_package.Enums;
using planner_server_package;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static planner_server_package.Events.CreateChatResponseEvent;

namespace planner_auth_service.Infrastructure.Service
{
    public class RabbitMqService : RabbitMQServiceBase
    {
        private readonly IPublisherService _publisherService;
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqService(
            IServiceScopeFactory scopeFactory,
            string hostname,
            string userName,
            string password,
            string prefix,
            IPublisherService publisherService,
            ILogger<RabbitMQServiceBase> logger,
            string updateProfileImageQueue,
            string queueCreateChatName,
            string getGoogleTokenExchange)
            : base(hostname, userName, password, prefix, logger)
        {
            _scopeFactory = scopeFactory;

            _publisherService = publisherService;

            AddQueue(updateProfileImageQueue, HandleUpdateProfileImageMessageAsync);
            AddQueue(queueCreateChatName, HandleCreateChatMessageAsync);
            AddQueue(getGoogleTokenExchange, HandleGoogleTokenRequest);

            InitializeRabbitMQ();
        }


        private async Task<ServiceResponse<object>> HandleUpdateProfileImageMessageAsync(string message)
        {
            using var scope = _scopeFactory.CreateScope();
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
            using var scope = _scopeFactory.CreateScope();
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
            using var scope = _scopeFactory.CreateScope();
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
    }
}