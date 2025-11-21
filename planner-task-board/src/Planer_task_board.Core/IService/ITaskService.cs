using System.Net;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<TaskBody>> CreateOrUpdateTask(Guid accountId, CreateOrUpdateTaskBody taskBody);
        Task<ServiceResponse<List<TaskBody>>> CreateOrUpdateTasks(Guid accountId, List<CreateOrUpdateTaskBody> taskBodies);
        Task<ServiceResponse<DeletedTaskBody>> RemoveTask(Guid accountId, Guid boardId, Guid taskId);
        Task<HttpStatusCode> RestoreDeletedTask(Guid deletedTaskId, Guid boardId, Guid accountId);
        Task<ServiceResponse<IEnumerable<DeletedTaskBody>>> GetDeletedTasks(Guid accountId, Guid boardId);
        Task<ServiceResponse<IEnumerable<TaskBody>>> GetTasks(Guid accountId, Guid boardId, Guid columnId, TaskState? state);
        Task<ServiceResponse<IEnumerable<TaskBody>>> GetAllTasks(Guid accountId);
        Task<ServiceResponse<IEnumerable<Guid>>> GetTaskPerformerIds(Guid accountId, Guid boardId, Guid taskId, int count, int offset);
        Task<HttpStatusCode> AddTaskPerformers(Guid accountId, Guid taskId, Guid boardId, IEnumerable<Guid> performerIds);
        Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, UpdateTaskBody taskBody);
        Task<ServiceResponse<List<TaskBody>>> UpdateTasks(Guid accountId, List<UpdateTaskBody> taskBodies);
        Task<HttpStatusCode> AddTaskToColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        Task<HttpStatusCode> RemoveTaskFromColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        //Task<ServiceResponse<IEnumerable<BoardColumnTaskBody>>> GetColumnTaskMembership(Guid accountId);
        //Task<ServiceResponse<BoardColumnTaskBody>> UpdateColumnTaskMembership(Guid accountId, BoardColumnTaskBody columnTaskMembership);
        //Task<ServiceResponse<List<BoardColumnTaskBody>>> UpdateColumnTaskMemberships(Guid accountId, List<BoardColumnTaskBody> columnTaskMemberships);
        //Task<ServiceResponse<IEnumerable<TaskAttachedMessageBody>>> GetTasksAttachedMessages(Guid accountId);
    }
}