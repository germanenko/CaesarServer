using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<TaskBody?> AddAsync<T>(T jobBody, Guid accountId) where T : JobBody;
        Task<TaskBody?> AddJobFromMessageAsync<T>(T jobBody, Guid accountId, Guid messageId, string snapshot) where T : JobBody;
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