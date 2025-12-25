using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskModel?> AddAsync(TaskBody task, Guid accountId);
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
            TaskBody updatedNode,
            Guid? columnId,
            DateTime changeDate);
        Task<TaskModel?> UpdateAsync(
            Guid id,
            Guid accountId,
            TaskBody updatedNode,
            DateTime changeDate);
        Task AssignTaskToColumn(Guid task, Guid columnId);
        Task<bool> RemoveTaskFromColumn(Guid taskId, Guid columnId);
        Task<NodeLink?> UpdateTaskChatId(Guid taskId, Guid chatId);
        //Task<IEnumerable<TaskAttachedMessage>> GetTasksAttachedMessages(Guid accountId);
    }
}