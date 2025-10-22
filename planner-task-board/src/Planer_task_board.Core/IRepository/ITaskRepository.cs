using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskModel?> AddAsync(
            Guid id,
            string title,
            string description,
            int priorityOrder,
            TaskState taskState,
            TaskType taskType,
            DateTime? startDate,
            DateTime? endDate,
            string? hexColor,
            BoardColumn column,
            Guid creatorId,
            List<Guid> messages,
            DateTime changeDate);
        Task<TaskModel?> AddAsync(
            Guid id,
            string title,
            string description,
            string? hexColor,
            TaskType taskType,
            DateTime? startDate,
            DateTime? endDate,
            BoardColumn column,
            Guid creatorId,
            List<Guid> messages,
            TaskModel? parentTask,
            DateTime changeDate);
        Task<TaskModel?> GetAsync(Guid id, bool isDraft);
        Task<bool> RemoveAsync(Guid id, bool isDraft);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, TaskState? status, bool isDraft = false);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, bool isDraft = false);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId, Guid userId);
        Task<IEnumerable<TaskModel>> GetAll(Guid columnId);
        Task<IEnumerable<TaskModel>> GetAllTasks(Guid accountId);
        Task<TaskModel?> ConvertDraftToTask(Guid id, Guid accountId, BoardColumn column);
        Task<TaskModel?> UpdateAsync(
            Guid id,
            string title,
            string description,
            int priorityOrder,
            TaskState taskState,
            DateTime? startDate,
            DateTime? endDate,
            string? hexColor,
            Guid columnId,
            DateTime changeDate);
        Task<TaskModel?> UpdateAsync(
            Guid id,
            string title,
            string description,
            DateTime? startDate,
            DateTime? endDate,
            string? hexColor,
            TaskModel? draftOfTask,
            DateTime changeDate);
        Task<TaskPerformer?> LinkPerformerToTaskAsync(TaskModel task, Guid performerId);
        Task<IEnumerable<TaskPerformer>> LinkPerformersToTaskAsync(TaskModel task, IEnumerable<Guid> performerIds);
        Task<TaskPerformer?> GetTaskPerformer(Guid performerId, Guid taskId);
        Task<IEnumerable<TaskPerformer>> GetTaskPerformers(IEnumerable<Guid> performerIds, Guid taskId, int count, int offset);
        Task<IEnumerable<TaskPerformer>> GetTaskPerformers(Guid taskId, int count, int offset);
        Task AssignTaskToColumn(TaskModel task, BoardColumn column);
        Task<bool> RemoveTaskFromColumn(Guid taskId, Guid columnId);
        Task<TaskModel?> UpdateTaskChatId(Guid taskId, Guid chatId);
        Task<IEnumerable<BoardColumnTask>> GetColumnTaskMembership(Guid accountId);
    }
}