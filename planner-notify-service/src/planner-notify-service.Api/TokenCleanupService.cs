using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_notify_service.Core.IRepository;
using planner_notify_service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.App.Service
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

        // ИСПРАВЛЕНИЕ: Используем IServiceProvider вместо INotifyRepository
        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CleanUpInactiveDevicesAsync()
        {
            // ИСПРАВЛЕНИЕ: Создаем scope для каждого вызова
            using var scope = _serviceProvider.CreateScope();
            var notifyRepository = scope.ServiceProvider.GetRequiredService<INotifyRepository>();
            await notifyRepository.DeleteInvalidTokens();
        }
    }
}
