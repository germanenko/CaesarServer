using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_server_package.Idempotency.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency
{
    public class OperationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OperationCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
        public OperationCleanupService(IServiceProvider serviceProvider, ILogger<OperationCleanupService> logger)
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
                    await CleanUpInactiveDevicesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Произошла ошибка при очистке зависших запросов.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CleanUpInactiveDevicesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var notifyRepository = scope.ServiceProvider.GetRequiredService<IIdempotencyRepository>();
            await notifyRepository.DeleteStuckRequests();
        }
    }
}
