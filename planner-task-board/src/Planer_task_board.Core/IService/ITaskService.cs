using System.Net;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<Node>> CreateOrUpdateTask(Guid accountId, Node taskBody);
        Task<ServiceResponse<List<Node>>> CreateOrUpdateTasks(Guid accountId, List<Node> taskBodies);
        Task<ServiceResponse<Node>> RemoveTask(Guid accountId, Guid boardId, Guid taskId);
        Task<HttpStatusCode> RestoreDeletedTask(Guid deletedTaskId, Guid boardId, Guid accountId);
        Task<ServiceResponse<IEnumerable<Node>>> GetDeletedTasks(Guid accountId, Guid boardId);
        Task<ServiceResponse<IEnumerable<Node>>> GetTasks(Guid accountId, Guid boardId, Guid columnId, WorkflowStatus? state);
        Task<ServiceResponse<IEnumerable<Node>>> GetAllTasks(Guid accountId);
        Task<ServiceResponse<Node>> UpdateTask(Guid accountId, Node taskBody);
        Task<ServiceResponse<List<Node>>> UpdateTasks(Guid accountId, List<Node> taskBodies);
        Task<HttpStatusCode> AddTaskToColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        Task<HttpStatusCode> RemoveTaskFromColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        //Task<ServiceResponse<IEnumerable<BoardColumnTaskBody>>> GetColumnTaskMembership(Guid accountId);
        //Task<ServiceResponse<BoardColumnTaskBody>> UpdateColumnTaskMembership(Guid accountId, BoardColumnTaskBody columnTaskMembership);
        //Task<ServiceResponse<List<BoardColumnTaskBody>>> UpdateColumnTaskMemberships(Guid accountId, List<BoardColumnTaskBody> columnTaskMemberships);
        //Task<ServiceResponse<IEnumerable<TaskAttachedMessageBody>>> GetTasksAttachedMessages(Guid accountId);
    }
}