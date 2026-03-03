using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using planner_server_package.RabbitMQ;

namespace planner_analytics_service.Infrastructure.Service
{
    public class RabbitMqService : RabbitMQServiceBase
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string userName,
            string password,
            string prefix,
            ILogger<RabbitMQServiceBase> logger)
            : base(hostname, userName, password, prefix, logger)
        {
            _scopeFactory = serviceFactory;

            InitializeRabbitMQ();
        }
    }
}