using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_mailbox_service.Core.Entities.Models;
using planner_mailbox_service.Core.Entities.Request;
using planner_mailbox_service.Infrastructure.Data;
using planner_server_package.Entities;
using planner_server_package.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace planner_mailbox_service.Infrastructure.Service
{
    public class RabbitMqService : RabbitMQServiceBase
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqService(
            IServiceScopeFactory scopeFactory,
            string hostname,
            string userName,
            string password,
            string prefix,
            ILogger<RabbitMQServiceBase> logger,
            string queueName)
            : base(hostname, userName, password, prefix, logger)
        {
            _scopeFactory = scopeFactory;

            AddQueue(queueName, HandleMessageAsync);

            InitializeRabbitMQ();
        }


        private async Task<ServiceResponse<object>> HandleMessageAsync(string message)
        {
            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AccountCredentialsDbContext>();
            var mailCredentials = JsonSerializer.Deserialize<AccountMailCredentialsDto>(message);

            if (mailCredentials == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };


            var account = await context.MailCredentials.FirstOrDefaultAsync(x => x.Email == mailCredentials.Email
                                                                                 && x.EmailProvider == mailCredentials.EmailProvider.ToString());
            if (account != null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            account = new AccountMailCredentials
            {
                Email = mailCredentials.Email,
                EmailProvider = mailCredentials.EmailProvider.ToString(),
                AccessToken = mailCredentials.AccessToken,
                RefreshToken = mailCredentials.RefreshToken,
                AccountId = mailCredentials.AccountId,
            };

            await context.MailCredentials.AddAsync(account);
            await context.SaveChangesAsync();

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}