using System.Net;
using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;

namespace Planer_task_board.App.Service
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IPublicationStatusRepository _publicationStatusRepository;

        public TaskService(
            ITaskRepository taskRepository,
            IBoardRepository boardRepository,
            IPublicationStatusRepository publicationStatusRepository)
        {
            _taskRepository = taskRepository;
            _boardRepository = boardRepository;
            _publicationStatusRepository = publicationStatusRepository;
        }


        public async Task<HttpStatusCode> AddTaskToColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return HttpStatusCode.Forbidden;

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return HttpStatusCode.BadRequest;

            var userBoards = await _boardRepository.GetAll(accountId);
            var boardIds = userBoards.Select(e => e.Id);
            if (!boardIds.Contains(boardId))
                return HttpStatusCode.Forbidden;

            var task = await _taskRepository.GetAsync(taskId, false);
            if (task == null)
                return HttpStatusCode.BadRequest;

            await _taskRepository.AssignTaskToColumn(task.Id, column.Id);
            return HttpStatusCode.OK;
        }

        public async Task<ServiceResponse<TaskBody>> CreateOrUpdateTask(Guid accountId, TaskBody taskBody)
        {
            var errors = new List<string>();
            if (taskBody.StartDate != null && !DateTime.TryParse(taskBody?.StartDate, out var _))
                errors.Add("Start time format is not correct");

            if (taskBody.EndDate != null && !DateTime.TryParse(taskBody.EndDate, out var _))
                errors.Add("End time format is not correct");


            var column = await _boardRepository.GetBoardColumnByChild(taskBody.Id);

            DateTime? startDate = taskBody.StartDate == null ? null : DateTime.Parse(taskBody.StartDate);
            DateTime? endDate = taskBody.EndDate == null ? null : DateTime.Parse(taskBody.EndDate);

            if (await _taskRepository.GetAsync(taskBody.Id, false) != null)
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
                Body = result.ToTaskBody(),
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

        public async Task<ServiceResponse<IEnumerable<TaskBody>>> GetDeletedTasks(Guid accountId, Guid boardId)
        {

            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<IEnumerable<TaskBody>>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new string[] { "You are not a member of this board" },
                    IsSuccess = false
                };

            var deletedTasks = await _publicationStatusRepository.Get(PublicationStatus.Deleted);
            return new ServiceResponse<IEnumerable<TaskBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = deletedTasks.Select(t => ((TaskModel)t.Node).ToTaskBody()),
                IsSuccess = true
            };
        }


        public async Task<ServiceResponse<IEnumerable<TaskBody>>> GetTasks(Guid accountId, Guid boardId, Guid columnId, WorkflowStatus? state)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<IEnumerable<TaskBody>>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new string[] { "You are not a member of this board" },
                    IsSuccess = false
                };

            var tasks = state == null
                ? await _taskRepository.GetAll(columnId, false)
                : await _taskRepository.GetAll(columnId, state.Value, false);

            return new ServiceResponse<IEnumerable<TaskBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = tasks.Select(x => x.ToTaskBody()),
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<IEnumerable<TaskBody>>> GetAllTasks(Guid accountId)
        {
            var tasks = await _taskRepository.GetAllTasks(accountId);

            return new ServiceResponse<IEnumerable<TaskBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = tasks.Select(x => x.ToTaskBody()),
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<TaskBody>> RemoveTask(Guid accountId, Guid boardId, Guid taskId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<TaskBody>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new string[] { "You are not a member of this board" },
                    IsSuccess = false
                };

            var task = await _taskRepository.GetAsync(taskId, true);
            if (task == null)
                return new ServiceResponse<TaskBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new string[] { "Task id isn't exist" },
                    IsSuccess = false
                };

            var deletedTask = await _publicationStatusRepository.ChangeStatus(task.Id, PublicationStatus.Deleted);
            return deletedTask == null ? new ServiceResponse<TaskBody>
            {
                StatusCode = HttpStatusCode.Conflict,
                Errors = new string[] { "Task deleted exist" },
                IsSuccess = false
            } : new ServiceResponse<TaskBody>
            {
                StatusCode = HttpStatusCode.OK,
                Body = ((TaskModel)deletedTask.Node).ToTaskBody(),
                IsSuccess = true
            };
        }

        public async Task<HttpStatusCode> RemoveTaskFromColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return HttpStatusCode.Forbidden;

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return HttpStatusCode.BadRequest;

            var userBoards = await _boardRepository.GetAll(accountId);
            var boardIds = userBoards.Select(e => e.Id);
            if (!boardIds.Contains(boardId))
                return HttpStatusCode.Forbidden;

            var task = await _taskRepository.GetAsync(taskId, true);
            if (task == null)
                return HttpStatusCode.BadRequest;

            await _taskRepository.RemoveTaskFromColumn(taskId, columnId);
            return HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> RestoreDeletedTask(Guid deletedTaskId, Guid boardId, Guid accountId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return HttpStatusCode.Forbidden;

            var result = await _publicationStatusRepository.ChangeStatus(deletedTaskId, PublicationStatus.Active);
            return result != null ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;
        }

        public async Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, TaskBody taskBody)
        {
            var errors = new List<string>();

            if (taskBody.StartDate != null && !DateTime.TryParse(taskBody?.StartDate, out var _))
                errors.Add("Start time format is not correct");

            if (taskBody.EndDate != null && !DateTime.TryParse(taskBody.EndDate, out var _))
                errors.Add("End time format is not correct");

            if (errors.Any())
                return new ServiceResponse<TaskBody>
                {
                    Errors = errors.ToArray(),
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            DateTime? startDate = taskBody.StartDate == null ? null : DateTime.Parse(taskBody.StartDate);
            DateTime? endDate = taskBody.EndDate == null ? null : DateTime.Parse(taskBody.EndDate);

            var result = await _taskRepository.UpdateAsync(taskBody.Id, accountId, taskBody, DateTime.UtcNow);
            return result == null ? new ServiceResponse<TaskBody>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Errors = new string[] { "Task not updated" },
                IsSuccess = false
            } : new ServiceResponse<TaskBody>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result.ToTaskBody(),
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<List<TaskBody>>> UpdateTasks(Guid accountId, List<TaskBody> taskBodies)
        {
            var errors = new List<string>();
            List<TaskBody> result = new List<TaskBody>();
            foreach (var taskBody in taskBodies)
            {
                if (taskBody.StartDate != null && !DateTime.TryParse(taskBody?.StartDate, out var _))
                    errors.Add("Start time format is not correct");

                if (taskBody.EndDate != null && !DateTime.TryParse(taskBody.EndDate, out var _))
                    errors.Add("End time format is not correct");

                if (errors.Any())
                    return new ServiceResponse<List<TaskBody>>
                    {
                        Errors = errors.ToArray(),
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false
                    };

                DateTime? startDate = taskBody.StartDate == null ? null : DateTime.Parse(taskBody.StartDate);
                DateTime? endDate = taskBody.EndDate == null ? null : DateTime.Parse(taskBody.EndDate);

                result.Add((await _taskRepository.UpdateAsync(taskBody.Id, accountId, taskBody, DateTime.UtcNow)).ToTaskBody());
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