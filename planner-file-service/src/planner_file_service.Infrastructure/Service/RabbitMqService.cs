using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_common_package.Enums;
using planner_file_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace planner_file_service.Infrastructure.Service
{
    public class RabbitMqService : RabbitMQServiceBase
    {
        private readonly IPublisherService _publisherService;

        public RabbitMqService(
            string hostname,
            string userName,
            string password,
            string prefix,
            IPublisherService publisherService,
            ILogger<RabbitMQServiceBase> logger)
            : base(hostname, userName, password, prefix, logger)
        {
            _publisherService = publisherService;

            InitializeRabbitMQ();
        }
    }
}