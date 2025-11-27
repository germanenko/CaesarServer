using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Planner_Auth.Core.IRepository;
using Planner_Auth.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_Auth.App.Service
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Ждем немного перед первым запуском при старте приложения
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Запуск фоновой задачи по очистке неактивных устройств...");
                    await CleanUpInactiveDevicesAsync();
                    _logger.LogInformation("Очистка неактивных устройств завершена.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Произошла ошибка при очистке неактивных устройств.");
                }

                // Ожидаем до следующего запуска
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CleanUpInactiveDevicesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            await accountRepository.DeleteInvalidSessions();
        }
    }
}
