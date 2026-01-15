using System.Net;
using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;

namespace planner_content_service.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<TaskBody>> CreateOrUpdateTask(Guid accountId, TaskBody taskBody);
        Task<ServiceResponse<List<TaskBody>>> CreateOrUpdateTasks(Guid accountId, List<TaskBody> taskBodies);
        Task<ServiceResponse<TaskBody>> RemoveTask(Guid accountId, Guid boardId, Guid taskId);
        Task<HttpStatusCode> RestoreDeletedTask(Guid deletedTaskId, Guid boardId, Guid accountId);
        Task<ServiceResponse<IEnumerable<TaskBody>>> GetDeletedTasks(Guid accountId, Guid boardId);
        Task<ServiceResponse<IEnumerable<TaskBody>>> GetTasks(Guid accountId, Guid boardId, Guid columnId, WorkflowStatus? state);
        Task<ServiceResponse<IEnumerable<TaskBody>>> GetAllTasks(Guid accountId);
        Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, TaskBody taskBody);
        Task<ServiceResponse<List<TaskBody>>> UpdateTasks(Guid accountId, List<TaskBody> taskBodies);
        Task<HttpStatusCode> AddTaskToColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        Task<HttpStatusCode> RemoveTaskFromColumn(Guid accountId, Guid boardId, Guid taskId, Guid columnId);
        //Task<ServiceResponse<IEnumerable<BoardColumnTaskBody>>> GetColumnTaskMembership(Guid accountId);
        //Task<ServiceResponse<BoardColumnTaskBody>> UpdateColumnTaskMembership(Guid accountId, BoardColumnTaskBody columnTaskMembership);
        //Task<ServiceResponse<List<BoardColumnTaskBody>>> UpdateColumnTaskMemberships(Guid accountId, List<BoardColumnTaskBody> columnTaskMemberships);
        //Task<ServiceResponse<IEnumerable<TaskAttachedMessageBody>>> GetTasksAttachedMessages(Guid accountId);
    }
}