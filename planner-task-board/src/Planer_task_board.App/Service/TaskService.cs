using System.Net;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;

namespace Planer_task_board.App.Service
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IDeletedTaskRepository _deletedTaskRepository;

        public TaskService(
            ITaskRepository taskRepository,
            IBoardRepository boardRepository,
            IDeletedTaskRepository deletedTaskRepository)
        {
            _taskRepository = taskRepository;
            _boardRepository = boardRepository;
            _deletedTaskRepository = deletedTaskRepository;
        }

        public async Task<HttpStatusCode> AddTaskPerformers(Guid accountId, Guid taskId, Guid boardId, IEnumerable<Guid> performerIds)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return HttpStatusCode.Forbidden;

            var task = await _taskRepository.GetAsync(taskId, false);
            if (task == null)
                return HttpStatusCode.BadRequest;

            var boardMembers = await _boardRepository.GetBoardMembers(performerIds, boardId);
            var members = boardMembers.IntersectBy(performerIds, e => e.AccountId).Select(e => e.AccountId);
            if (!members.Any())
                return HttpStatusCode.Forbidden;

            var addedPerformers = await _taskRepository.LinkPerformersToTaskAsync(task, members);
            return HttpStatusCode.OK;
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

            await _taskRepository.AssignTaskToColumn(task, column);
            return HttpStatusCode.OK;
        }

        public async Task<ServiceResponse<TaskBody>> CreateOrUpdateTask(Guid accountId, CreateOrUpdateTaskBody taskBody)
        {
            var errors = new List<string>();
            if (taskBody.StartDate != null && !DateTime.TryParse(taskBody?.StartDate, out var _))
                errors.Add("Start time format is not correct");

            if (taskBody.EndDate != null && !DateTime.TryParse(taskBody.EndDate, out var _))
                errors.Add("End time format is not correct");

            //if(taskBody.ColumnId != null)
            //{
            //    var columnMember = await _boardRepository.GetColumnMemberAsync(accountId, taskBody.ColumnId);
            //    if (columnMember == null)
            //    {
            //        errors.Add("You are not a member of this column");
            //        return new ServiceResponse<TaskBody>
            //        {
            //            StatusCode = HttpStatusCode.Forbidden,
            //            Errors = errors.ToArray(),
            //            IsSuccess = false
            //        };
            //    }
            //}
            

            var column = await _boardRepository.GetBoardColumn(taskBody.ColumnId);

            DateTime? startDate = taskBody.StartDate == null ? null : DateTime.Parse(taskBody.StartDate);
            DateTime? endDate = taskBody.EndDate == null ? null : DateTime.Parse(taskBody.EndDate);

            if(await _taskRepository.GetAsync(taskBody.Id, false) != null)
            {
                var task = await UpdateTask(accountId, new UpdateTaskBody() 
                {
                    Id = taskBody.Id,
                    Title = taskBody.Title,
                    Description = taskBody.Description,
                    PriorityOrder = taskBody.PriorityOrder,
                    Status = taskBody.Status,
                    HexColor = taskBody.HexColor,
                    ColumnId = taskBody.ColumnId,
                    UpdatedAt = taskBody.UpdatedAt
                });

                return task;
            }

            var result = await _taskRepository.AddAsync(
                taskBody.Id,
                taskBody.Title,
                taskBody.Description,
                taskBody.PriorityOrder,
                taskBody.Status,
                taskBody.Type,
                startDate,
                endDate,
                taskBody.HexColor,
                column,
                accountId,
                taskBody.MessageIds,
                taskBody.UpdatedAt);
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

        public async Task<ServiceResponse<List<TaskBody>>> CreateOrUpdateTasks(Guid accountId, List<CreateOrUpdateTaskBody> taskBodies)
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

        public async Task<ServiceResponse<IEnumerable<DeletedTaskBody>>> GetDeletedTasks(Guid accountId, Guid boardId)
        {

            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<IEnumerable<DeletedTaskBody>>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new string[] { "You are not a member of this board" },
                    IsSuccess = false
                };

            var deletedTasks = await _deletedTaskRepository.GetAll();
            return new ServiceResponse<IEnumerable<DeletedTaskBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = deletedTasks.Select(t => t.ToDeletedTaskBody()),
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<IEnumerable<Guid>>> GetTaskPerformerIds(Guid accountId, Guid boardId, Guid taskId, int count, int offset)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<IEnumerable<Guid>>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new string[] { "You are not a member of this board" },
                    IsSuccess = false
                };

            var taskPerformers = await _taskRepository.GetTaskPerformers(taskId, count, offset);
            var performers = taskPerformers.Select(e => e.PerformerId);
            return new ServiceResponse<IEnumerable<Guid>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = performers,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<IEnumerable<TaskBody>>> GetTasks(Guid accountId, Guid boardId, Guid columnId, TaskState? state)
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
                Body = tasks.Select(e => e.ToTaskBody()),
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<IEnumerable<TaskBody>>> GetAllTasks(Guid accountId)
        {
            var tasks = await _taskRepository.GetAllTasks(accountId);

            return new ServiceResponse<IEnumerable<TaskBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = tasks.Select(a => a.ToTaskBody()),
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<DeletedTaskBody>> RemoveTask(Guid accountId, Guid boardId, Guid taskId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<DeletedTaskBody>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Errors = new string[] { "You are not a member of this board" },
                    IsSuccess = false
                };

            var task = await _taskRepository.GetAsync(taskId, true);
            if (task == null)
                return new ServiceResponse<DeletedTaskBody>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new string[] { "Task id isn't exist" },
                    IsSuccess = false
                };

            var deletedTask = await _deletedTaskRepository.AddAsync(task);
            return deletedTask == null ? new ServiceResponse<DeletedTaskBody>
            {
                StatusCode = HttpStatusCode.Conflict,
                Errors = new string[] { "Task deleted exist" },
                IsSuccess = false
            } : new ServiceResponse<DeletedTaskBody>
            {
                StatusCode = HttpStatusCode.OK,
                Body = deletedTask.ToDeletedTaskBody(),
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

            var result = await _deletedTaskRepository.RemoveAsync(deletedTaskId);
            return result != false ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest;
        }

        public async Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, UpdateTaskBody taskBody)
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

            //var columnMember = await _boardRepository.GetColumnMemberAsync(accountId, taskBody.ColumnId);
            //if (columnMember == null)
            //    return new ServiceResponse<TaskBody>
            //    {
            //        StatusCode = HttpStatusCode.Forbidden,
            //        Errors = new string[] { "You are not a member of this column" },
            //        IsSuccess = false
            //    };

            DateTime? startDate = taskBody.StartDate == null ? null : DateTime.Parse(taskBody.StartDate);
            DateTime? endDate = taskBody.EndDate == null ? null : DateTime.Parse(taskBody.EndDate);

            var result = await _taskRepository.UpdateAsync(taskBody.Id, taskBody.Title, taskBody.Description, taskBody.PriorityOrder, taskBody.Status, startDate, endDate, taskBody.HexColor, taskBody.ColumnId, taskBody.UpdatedAt);
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

        public async Task<ServiceResponse<List<TaskBody>>> UpdateTasks(Guid accountId, List<UpdateTaskBody> taskBodies)
        {
            var errors = new List<string>();
            List<TaskModel> result = new List<TaskModel>();
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

                //var columnMember = await _boardRepository.GetColumnMemberAsync(accountId, taskBody.ColumnId);
                //if (columnMember == null)
                //    return new ServiceResponse<List<TaskBody>>
                //    {
                //        StatusCode = HttpStatusCode.Forbidden,
                //        Errors = new string[] { "You are not a member of this column" },
                //        IsSuccess = false
                //    };

                DateTime? startDate = taskBody.StartDate == null ? null : DateTime.Parse(taskBody.StartDate);
                DateTime? endDate = taskBody.EndDate == null ? null : DateTime.Parse(taskBody.EndDate);

                result.Add(await _taskRepository.UpdateAsync(taskBody.Id, taskBody.Title, taskBody.Description, taskBody.PriorityOrder, taskBody.Status, startDate, endDate, taskBody.HexColor, taskBody.ColumnId, taskBody.UpdatedAt));
            }
            
            return result.Count != taskBodies.Count ? new ServiceResponse<List<TaskBody>>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Errors = new string[] { "Not all tasks updated" },
                IsSuccess = false
            } : new ServiceResponse<List<TaskBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = result.Select(x => x.ToTaskBody()).ToList(),
                IsSuccess = true
            };
        }

        //public async Task<ServiceResponse<IEnumerable<BoardColumnTaskBody>>> GetColumnTaskMembership(Guid accountId)
        //{
        //    var memberships = await _taskRepository.GetColumnTaskMembership(accountId);

        //    return new ServiceResponse<IEnumerable<BoardColumnTaskBody>>
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Body = memberships.Select(x => x.ToBoardColumnTaskBody()),
        //        IsSuccess = true
        //    };
        //}

        //public async Task<ServiceResponse<BoardColumnTaskBody>> UpdateColumnTaskMembership(Guid accountId, BoardColumnTaskBody columnTaskMembership)
        //{
        //    //var columnMember = await _boardRepository.GetColumnMemberAsync(accountId, columnTaskMembership.ColumnId);
        //    //if (columnMember == null)
        //    //    return new ServiceResponse<BoardColumnTaskBody>
        //    //    {
        //    //        StatusCode = HttpStatusCode.Forbidden,
        //    //        Errors = new string[] { "You are not a member of this column" },
        //    //        IsSuccess = false
        //    //    };


        //    var result = await _taskRepository.UpdateColumnTaskMembership(columnTaskMembership);
        //    return result == null ? new ServiceResponse<BoardColumnTaskBody>
        //    {
        //        StatusCode = HttpStatusCode.BadRequest,
        //        Errors = new string[] { "ColumnTaskMembership not updated" },
        //        IsSuccess = false
        //    } : new ServiceResponse<BoardColumnTaskBody>
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Body = result.ToBoardColumnTaskBody(),
        //        IsSuccess = true
        //    };
        //}

        //public async Task<ServiceResponse<List<BoardColumnTaskBody>>> UpdateColumnTaskMemberships(Guid accountId, List<BoardColumnTaskBody> columnTaskMemberships)
        //{
        //    List<BoardColumnTaskBody> result = new List<BoardColumnTaskBody>();

        //    foreach (var membership in columnTaskMemberships)
        //    {
        //        var columnTask = await UpdateColumnTaskMembership(accountId, membership);

        //        if(columnTask != null)
        //            result.Add(columnTask.Body);
        //    }

            
        //    return result.Count == 0 ? new ServiceResponse<List<BoardColumnTaskBody>>
        //    {
        //        StatusCode = HttpStatusCode.BadRequest,
        //        Errors = new string[] { "ColumnTaskMemberships not updated" },
        //        IsSuccess = false
        //    } : new ServiceResponse<List<BoardColumnTaskBody>>
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Body = result,
        //        IsSuccess = true
        //    };
        //}

        public async Task<ServiceResponse<IEnumerable<TaskAttachedMessageBody>>> GetTasksAttachedMessages(Guid accountId)
        {

            var tasksAttachedMessages = await _taskRepository.GetTasksAttachedMessages(accountId);

            return new ServiceResponse<IEnumerable<TaskAttachedMessageBody>>
            {
                StatusCode = HttpStatusCode.OK,
                Body = tasksAttachedMessages.Select(t => t.ToTaskAttachedMessageBody()),
                IsSuccess = true
            };
        }
    }
}