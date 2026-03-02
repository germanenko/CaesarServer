using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Net;
using System.Threading.Tasks;

namespace planner_content_service.App.Service
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IPublisherService _publisherService;

        public TaskService(
            ITaskRepository taskRepository,
            IBoardRepository boardRepository,
            IPublisherService publisherService)
        {
            _taskRepository = taskRepository;
            _publisherService = publisherService;
        }

        public async Task<ServiceResponse<TaskBody>> CreateOrUpdateTask(Guid accountId, TaskBody taskBody)
        {
            var errors = new List<string>();
            if (taskBody.StartDate != null /*&& !DateTime.TryParse(taskBody?.StartDate, out var _) */)
                errors.Add("Start time format is not correct");

            if (taskBody.EndDate != null /*&& !DateTime.TryParse(taskBody.EndDate, out var _)*/)
                errors.Add("End time format is not correct");


            DateTime? startDate = taskBody.StartDate /*== null ? null : DateTime.Parse(taskBody.StartDate)*/;
            DateTime? endDate = taskBody.EndDate /*== null ? null : DateTime.Parse(taskBody.EndDate) */;


            CreateTaskEvent taskEvent = new CreateTaskEvent()
            {
                Task = BodyConverter.ClientToServerBody(taskBody),
                CreatorId = accountId
            };

            var nodeComplete = await _publisherService.Publish(taskEvent, PublishEvent.CreateTask);

            if (!nodeComplete.IsSuccess)
            {
                return new ServiceResponse<TaskBody>
                {
                    IsSuccess = nodeComplete.IsSuccess,
                    StatusCode = nodeComplete.StatusCode,
                    Errors = nodeComplete.Errors
                };
            }

            if (await _taskRepository.GetAsync(taskBody.Id) != null)
            {
                var task = await UpdateTask(accountId, taskBody);

                return task;
            }

            var result = await _taskRepository.AddAsync(taskBody, accountId);

            if (result == null)
            {
                errors.Add("Task not created");
                return new ServiceResponse<TaskBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = errors.ToArray(),
                    IsSuccess = false
                };
            }

            return new ServiceResponse<TaskBody>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<List<TaskBody>>> CreateOrUpdateTasks(Guid accountId, List<TaskBody> taskBodies)
        {
            var errors = new List<string>();
            List<TaskBody> tasks = new List<TaskBody>();
            foreach (var taskBody in taskBodies)
            {
                var result = await CreateOrUpdateTask(accountId, taskBody);

                if (result.IsSuccess)
                {
                    tasks.Add(result.Body);
                }
                else
                {
                    return new ServiceResponse<List<TaskBody>>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Errors = result.Errors
                    };
                }
            }

            return new ServiceResponse<List<TaskBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = tasks,
                IsSuccess = true
            };
        }



        public async Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, TaskBody taskBody)
        {
            var errors = new List<string>();

            if (taskBody.StartDate != null/* && !DateTime.TryParse(taskBody?.StartDate, out var _) */)
                errors.Add("Start time format is not correct");

            if (taskBody.EndDate != null/* && !DateTime.TryParse(taskBody.EndDate, out var _) */)
                errors.Add("End time format is not correct");

            if (errors.Any())
                return new ServiceResponse<TaskBody>
                {
                    Errors = errors.ToArray(),
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            DateTime? startDate = taskBody.StartDate/* == null ? null : DateTime.Parse(taskBody.StartDate) */;
            DateTime? endDate = taskBody.EndDate/* == null ? null : DateTime.Parse(taskBody.EndDate) */;

            var result = await _taskRepository.UpdateAsync(taskBody.Id, accountId, taskBody, DateTime.UtcNow);
            return result == null ? new ServiceResponse<TaskBody>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Errors = new string[] { "Task not updated" },
                IsSuccess = false
            } : new ServiceResponse<TaskBody>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<List<TaskBody>>> UpdateTasks(Guid accountId, List<TaskBody> taskBodies)
        {
            var errors = new List<string>();
            List<TaskBody> result = new List<TaskBody>();
            foreach (var taskBody in taskBodies)
            {
                if (taskBody.StartDate != null/* && !DateTime.TryParse(taskBody?.StartDate, out var _)*/)
                    errors.Add("Start time format is not correct");

                if (taskBody.EndDate != null/* && !DateTime.TryParse(taskBody.EndDate, out var _)*/)
                    errors.Add("End time format is not correct");

                if (errors.Any())
                    return new ServiceResponse<List<TaskBody>>
                    {
                        Errors = errors.ToArray(),
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false
                    };

                DateTime? startDate = taskBody.StartDate/* == null ? null : DateTime.Parse(taskBody.StartDate)*/;
                DateTime? endDate = taskBody.EndDate/* == null ? null : DateTime.Parse(taskBody.EndDate)*/;

                result.Add(await _taskRepository.UpdateAsync(taskBody.Id, accountId, taskBody, DateTime.UtcNow));
            }

            return result.Count != taskBodies.Count ? new ServiceResponse<List<TaskBody>>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Errors = new string[] { "Not all tasks updated" },
                IsSuccess = false
            } : new ServiceResponse<List<TaskBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result,
                IsSuccess = true
            };
        }
    }
}