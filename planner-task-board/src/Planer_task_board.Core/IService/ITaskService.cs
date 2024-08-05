using System.Net;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<TaskBody>> CreateTask(Guid accountId, Guid boardId, Guid columnId, CreateTaskBody taskBody);
        Task<ServiceResponse<DeletedTaskBody>> RemoveTask(Guid accountId, Guid boardId, Guid taskId);
        Task<HttpStatusCode> RestoreDeletedTask(Guid deletedTaskId, Guid boardId, Guid accountId);
        Task<ServiceResponse<IEnumerable<DeletedTaskBody>>> GetDeletedTasks(Guid accountId, Guid boardId);
        Task<ServiceResponse<IEnumerable<TaskBody>>> GetTasks(Guid accountId, Guid boardId, Guid columnId, TaskState? state);
        Task<ServiceResponse<IEnumerable<Guid>>> GetTaskPerformerIds(Guid accountId, Guid boardId, Guid taskId, int count, int offset);
        Task<HttpStatusCode> AddTaskPerformers(Guid accountId, Guid taskId, Guid boardId, IEnumerable<Guid> performerIds);
        Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, Guid boardId, UpdateTaskBody taskBody);
        Task<HttpStatusCode> AddTaskToColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        Task<HttpStatusCode> RemoveTaskFromColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
    }
}