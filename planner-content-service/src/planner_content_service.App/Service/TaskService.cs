using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_server_package;
using planner_server_package.Access;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Net;

namespace planner_content_service.App.Service
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IPublisherService _publisherService;
        private readonly IAccessService _accessService;

        public TaskService(
            ITaskRepository taskRepository,
            IBoardRepository boardRepository,
            IPublisherService publisherService,
            IAccessService accessService)
        {
            _taskRepository = taskRepository;
            _publisherService = publisherService;
            _accessService = accessService;
        }

        public async Task<ServiceResponse<JobBody>> CreateOrUpdateJobFromMessage<T>(Guid accountId, T createOrUpdateJobBody, string snapshot, Guid messageId, CancellationToken cancellationToken = default) where T : JobBodyRequest
        {
            var hasAccess = await _accessService.CheckAccess(accountId, messageId, Permission.Read);

            if (!hasAccess)
            {
                return new ServiceResponse<JobBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Forbidden,
                    ErrorCodes = [ErrorCode.WriteDenied]
                };
            }

            var taskBody = new JobBody()
            {
                Id = createOrUpdateJobBody.Id,
                Name = createOrUpdateJobBody.Name,
                Description = createOrUpdateJobBody.Description,
                Link = createOrUpdateJobBody.Link,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = accountId,
                Type = NodeType.Job
            };

            CreateNodeEvent taskEvent = new CreateNodeEvent()
            {
                Node = BodyConverter.ClientToServerBody(taskBody),
                CreatorId = accountId
            };

            var nodeComplete = await _publisherService.Publish(taskEvent, PublishEvent.CreateNode);

            if (!nodeComplete.IsSuccess)
            {
                return new ServiceResponse<JobBody>
                {
                    IsSuccess = nodeComplete.IsSuccess,
                    StatusCode = nodeComplete.StatusCode,
                    Errors = nodeComplete.Errors
                };
            }

            if (await _taskRepository.GetAsync(createOrUpdateJobBody.Id, cancellationToken) != null)
            {
                var task = await UpdateTask(accountId, taskBody, cancellationToken);

                return task;
            }

            var result = await _taskRepository.AddJobFromMessageAsync(createOrUpdateJobBody, accountId, messageId, snapshot, cancellationToken);

            if (result == null)
            {
                return new ServiceResponse<JobBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = ["Task not created"],
                    IsSuccess = false
                };
            }

            return new ServiceResponse<JobBody>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<JobBody>> CreateOrUpdateTask<T>(Guid accountId, T createOrUpdateJobBody, CancellationToken cancellationToken = default) where T : JobBodyRequest
        {
            var taskBody = new JobBody()
            {
                Id = createOrUpdateJobBody.Id,
                Name = createOrUpdateJobBody.Name,
                Description = createOrUpdateJobBody.Description,
                Link = createOrUpdateJobBody.Link,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = accountId,
                Type = NodeType.Job
            };

            CreateNodeEvent taskEvent = new CreateNodeEvent()
            {
                Node = BodyConverter.ClientToServerBody(taskBody),
                CreatorId = accountId
            };

            var nodeComplete = await _publisherService.Publish(taskEvent, PublishEvent.CreateNode);

            if (!nodeComplete.IsSuccess)
            {
                return new ServiceResponse<JobBody>
                {
                    IsSuccess = nodeComplete.IsSuccess,
                    StatusCode = nodeComplete.StatusCode,
                    Errors = nodeComplete.Errors
                };
            }

            if (await _taskRepository.GetAsync(createOrUpdateJobBody.Id, cancellationToken) != null)
            {
                var task = await UpdateTask(accountId, taskBody, cancellationToken);

                return task;
            }

            var result = await _taskRepository.AddAsync(createOrUpdateJobBody, accountId, cancellationToken);

            if (result == null)
            {
                return new ServiceResponse<JobBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = ["Task not created"],
                    IsSuccess = false
                };
            }

            return new ServiceResponse<JobBody>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<List<JobBody>>> CreateOrUpdateTasks<T>(Guid accountId, List<T> taskBodies, CancellationToken cancellationToken = default) where T : JobBodyRequest
        {
            var errors = new List<string>();
            List<JobBody> tasks = new List<JobBody>();
            foreach (var taskBody in taskBodies)
            {
                var result = await CreateOrUpdateTask(accountId, taskBody);

                if (result.IsSuccess)
                {
                    tasks.Add(result.Body);
                }
                else
                {
                    return new ServiceResponse<List<JobBody>>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Errors = result.Errors
                    };
                }
            }

            return new ServiceResponse<List<JobBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = tasks,
                IsSuccess = true
            };
        }



        public async Task<ServiceResponse<JobBody>> UpdateTask(Guid accountId, JobBody taskBody, CancellationToken cancellationToken = default)
        {
            var errors = new List<string>();

            if (taskBody.StartDate != null/* && !DateTime.TryParse(taskBody?.StartDate, out var _) */)
                errors.Add("Start time format is not correct");

            if (taskBody.EndDate != null/* && !DateTime.TryParse(taskBody.EndDate, out var _) */)
                errors.Add("End time format is not correct");

            if (errors.Any())
                return new ServiceResponse<JobBody>
                {
                    Errors = errors.ToArray(),
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            DateTime? startDate = taskBody.StartDate/* == null ? null : DateTime.Parse(taskBody.StartDate) */;
            DateTime? endDate = taskBody.EndDate/* == null ? null : DateTime.Parse(taskBody.EndDate) */;

            var result = await _taskRepository.UpdateAsync(taskBody.Id, accountId, taskBody, DateTime.UtcNow, cancellationToken);
            return result == null ? new ServiceResponse<JobBody>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Errors = new string[] { "Task not updated" },
                IsSuccess = false
            } : new ServiceResponse<JobBody>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<List<JobBody>>> UpdateTasks(Guid accountId, List<JobBody> taskBodies, CancellationToken cancellationToken = default)
        {
            var errors = new List<string>();
            List<JobBody> result = new List<JobBody>();
            foreach (var taskBody in taskBodies)
            {
                if (taskBody.StartDate != null/* && !DateTime.TryParse(taskBody?.StartDate, out var _)*/)
                    errors.Add("Start time format is not correct");

                if (taskBody.EndDate != null/* && !DateTime.TryParse(taskBody.EndDate, out var _)*/)
                    errors.Add("End time format is not correct");

                if (errors.Any())
                    return new ServiceResponse<List<JobBody>>
                    {
                        Errors = errors.ToArray(),
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false
                    };

                DateTime? startDate = taskBody.StartDate/* == null ? null : DateTime.Parse(taskBody.StartDate)*/;
                DateTime? endDate = taskBody.EndDate/* == null ? null : DateTime.Parse(taskBody.EndDate)*/;

                result.Add(await _taskRepository.UpdateAsync(taskBody.Id, accountId, taskBody, DateTime.UtcNow, cancellationToken));
            }

            return result.Count != taskBodies.Count ? new ServiceResponse<List<JobBody>>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Errors = new string[] { "Not all tasks updated" },
                IsSuccess = false
            } : new ServiceResponse<List<JobBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<AttachedMessageBody>> AttachMessage(Guid accountId, Guid jobId, Guid messageId, string snapshot, CancellationToken cancellationToken = default)
        {
            var existingAttachedMessage = await _taskRepository.GetAttachedMessage(jobId, messageId, cancellationToken);

            if (existingAttachedMessage != null)
            {
                return new ServiceResponse<AttachedMessageBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorCodes = [ErrorCode.AlreadyExist]
                };
            }

            var attachedMessage = await _taskRepository.AttachMessage(accountId, jobId, messageId, snapshot, cancellationToken);

            if (attachedMessage == null)
            {
                return new ServiceResponse<AttachedMessageBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorCodes = [ErrorCode.Infrastructure]
                };
            }

            return new ServiceResponse<AttachedMessageBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = attachedMessage.ToBody()
            };
        }

        public async System.Threading.Tasks.Task SetMessageEdited(Guid messageId, MessageState state, CancellationToken cancellationToken = default)
        {
            await _taskRepository.SetMessageEdited(messageId, state, cancellationToken);
        }

        public async Task<ServiceResponse<ReadStateBody>> ReadAttachedMessages(Guid accountId, Guid jobId, CancellationToken cancellationToken = default)
        {
            var readState = await _taskRepository.UpdateReadState(accountId, jobId, cancellationToken);

            return new ServiceResponse<ReadStateBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = readState
            };
        }
    }
}