using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Planer_mailbox_service.Core.Entities.Models;
using Planer_mailbox_service.Core.Entities.Request;
using Planer_mailbox_service.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Planer_mailbox_service.Infrastructure.Service
{
    public class RabbitMqService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string queueName,
            string userName,
            string password)
        {
            _hostname = hostname;
            _queueName = queueName;
            _userName = userName;
            _password = password;
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
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await HandleMessageAsync(message);
            };

            _channel.BasicConsume(
                queue: _queueName,
                autoAck: true,
                consumer: consumer);

            await Task.CompletedTask;
        }

        private async Task HandleMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AccountCredentialsDbContext>();
            var mailCredentials = JsonSerializer.Deserialize<AccountMailCredentialsDto>(message);

            if (mailCredentials == null)
                return;


            var account = await context.MailCredentials.FirstOrDefaultAsync(x => x.Email == mailCredentials.Email
                                                                                 && x.EmailProvider == mailCredentials.EmailProvider.ToString());
            if (account != null)
                return;

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
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}