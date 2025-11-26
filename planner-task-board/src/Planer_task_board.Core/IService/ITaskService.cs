using System.Net;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<NodeBody>> CreateOrUpdateTask(Guid accountId, CreateOrUpdateTaskBody taskBody);
        Task<ServiceResponse<List<NodeBody>>> CreateOrUpdateTasks(Guid accountId, List<CreateOrUpdateTaskBody> taskBodies);
        Task<ServiceResponse<NodeBody>> RemoveTask(Guid accountId, Guid boardId, Guid taskId);
        Task<HttpStatusCode> RestoreDeletedTask(Guid deletedTaskId, Guid boardId, Guid accountId);
        Task<ServiceResponse<IEnumerable<NodeBody>>> GetDeletedTasks(Guid accountId, Guid boardId);
        Task<ServiceResponse<IEnumerable<NodeBody>>> GetTasks(Guid accountId, Guid boardId, Guid columnId, WorkflowStatus? state);
        Task<ServiceResponse<IEnumerable<NodeBody>>> GetAllTasks(Guid accountId);
        Task<ServiceResponse<NodeBody>> UpdateTask(Guid accountId, CreateOrUpdateTaskBody taskBody);
        Task<ServiceResponse<List<NodeBody>>> UpdateTasks(Guid accountId, List<CreateOrUpdateTaskBody> taskBodies);
        Task<HttpStatusCode> AddTaskToColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        Task<HttpStatusCode> RemoveTaskFromColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        //Task<ServiceResponse<IEnumerable<BoardColumnTaskBody>>> GetColumnTaskMembership(Guid accountId);
        //Task<ServiceResponse<BoardColumnTaskBody>> UpdateColumnTaskMembership(Guid accountId, BoardColumnTaskBody columnTaskMembership);
        //Task<ServiceResponse<List<BoardColumnTaskBody>>> UpdateColumnTaskMemberships(Guid accountId, List<BoardColumnTaskBody> columnTaskMemberships);
        //Task<ServiceResponse<IEnumerable<TaskAttachedMessageBody>>> GetTasksAttachedMessages(Guid accountId);
    }
}