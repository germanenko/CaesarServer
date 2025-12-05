using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskModel?> AddAsync(CreateOrUpdateTaskBody task, Guid accountId);
        Task<TaskModel?> GetAsync(Guid id, bool isDraft);
        Task<bool> RemoveAsync(Guid id, bool isDraft);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, WorkflowStatus? status, bool isDraft = false);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, bool isDraft = false);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, Guid userId);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId);
        Task<IEnumerable<TaskModel>> GetAllTasks(Guid accountId);
        Task<TaskModel?> ConvertDraftToTask(Guid id, Guid accountId, Guid? columnId);
        Task<TaskModel?> UpdateAsync(
            Guid id,
            Guid accountId,
            CreateOrUpdateTaskBody updatedNode,
            Guid? columnId,
            DateTime changeDate);
        Task<TaskModel?> UpdateAsync(
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