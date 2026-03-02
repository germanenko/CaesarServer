using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using planner_common_package.Enums;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Events;
using planner_server_package.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace planner_content_service.Infrastructure.Service
{
    public class RabbitMqService : RabbitMQServiceBase
    {
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly IPublisherService _publisherService;

        public RabbitMqService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string userName,
            string password,
            string prefix,
            IPublisherService publisherService,
            ILogger<RabbitMQServiceBase> logger,
            string contentNodes)
            : base(hostname, userName, password, prefix, logger)
        {
            _serviceFactory = serviceFactory;

            _publisherService = publisherService;

            AddQueue(contentNodes, HandleContentNodes);

            InitializeRabbitMQ();
        }

        private async Task<ServiceResponse<object>> HandleContentNodes(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
            var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();
            var response = JsonSerializer.Deserialize<SyncEntitiesEvent>(message);
            if (response == null)
                return new ServiceResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Errors = new[] { "Ошибка сервера" }
                };

            var boards = response.Bodies.OfType<BoardBody>().ToList();
            var columns = response.Bodies.OfType<ColumnBody>().ToList();
            var tasks = response.Bodies.OfType<TaskBody>().ToList();

            var boardBodies = boards.Select(x => new planner_client_package.Entities.BoardBody()
            {
                Id = x.Id,
                Name = x.Name,
                Props = x.Props,
                PublicationStatus = x.PublicationStatus,
                Type = NodeType.Board,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            }).ToList();

            var columnBodies = columns.Select(x => new planner_client_package.Entities.ColumnBody()
            {
                Id = x.Id,
                Name = x.Name,
                Props = x.Props,
                PublicationStatus = x.PublicationStatus,
                Type = NodeType.Column,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            }).ToList();

            var taskBodies = tasks.Select(x => new planner_client_package.Entities.TaskBody()
            {
                Id = x.Id,
                Name = x.Name,
                Props = x.Props,
                PublicationStatus = x.PublicationStatus,
                Type = NodeType.Column,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Description = x.Description,
                HexColor = x.HexColor,
                PriorityOrder = x.PriorityOrder,
                Status = x.Status,
                TaskType = x.TaskType
            }).ToList();

            await boardService.CreateOrUpdateBoards(boardBodies, response.TokenPayload.AccountId);
            await boardService.CreateOrUpdateColumns(response.TokenPayload.AccountId, columnBodies);
            await taskService.CreateOrUpdateTasks(response.TokenPayload.AccountId, taskBodies);

            return new ServiceResponse<object>()
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }
    }
}