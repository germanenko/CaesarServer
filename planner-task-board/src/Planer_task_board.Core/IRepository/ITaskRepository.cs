using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<Node?> AddAsync(CreateOrUpdateTaskBody task, Guid accountId);
        Task<Node?> GetAsync(Guid id, bool isDraft);
        Task<bool> RemoveAsync(Guid id, bool isDraft);
        Task<IEnumerable<Node>> GetAll(Guid columnId, WorkflowStatus? status, bool isDraft = false);
        Task<IEnumerable<Node>> GetAll(Guid columnId, bool isDraft = false);
        Task<IEnumerable<Node>> GetAll(Guid columnId, Guid userId);
        Task<IEnumerable<Node>> GetAll(Guid columnId);
        Task<IEnumerable<Node>> GetAllTasks(Guid accountId);
        Task<Node?> ConvertDraftToTask(Guid id, Guid accountId, Guid? columnId);
        Task<Node?> UpdateAsync(
            Guid id,
            Guid accountId,
            CreateOrUpdateTaskBody updatedNode,
            Guid? columnId,
            DateTime changeDate);
        Task<Node?> UpdateAsync(
            Guid id,
            Guid accountId,
            CreateOrUpdateTaskBody updatedNode,
            DateTime changeDate);
        Task AssignTaskToColumn(Guid task, Guid columnId);
        Task<bool> RemoveTaskFromColumn(Guid taskId, Guid columnId);
        Task<NodeLink?> UpdateTaskChatId(Guid taskId, Guid chatId);
        //Task<IEnumerable<TaskAttachedMessage>> GetTasksAttachedMessages(Guid accountId);
    }
}