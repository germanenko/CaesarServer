using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskModel?> AddAsync(TaskBody task, Guid accountId);
        Task<TaskModel?> GetAsync(Guid id, bool isDraft);
        Task<bool> RemoveAsync(Guid id, bool isDraft);
        IEnumerable<TaskModel?>? GetAll(List<Guid> ids);
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
        //Task<IEnumerable<TaskAttachedMessage>> GetTasksAttachedMessages(Guid accountId);
    }
}