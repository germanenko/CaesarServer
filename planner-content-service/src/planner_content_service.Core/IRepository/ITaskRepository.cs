using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskBody?> AddAsync(TaskBody task, Guid accountId);
        Task<TaskBody?> GetAsync(Guid id);
        Task<bool> RemoveAsync(Guid id);
        IEnumerable<TaskBody?>? GetAll(List<Guid> ids);

        Task<TaskBody?> UpdateAsync(
            Guid id,
            Guid accountId,
            TaskBody updatedNode,
            DateTime changeDate);
    }
}