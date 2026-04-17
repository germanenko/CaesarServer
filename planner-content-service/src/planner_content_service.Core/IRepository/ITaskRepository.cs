using planner_client_package.Entities;
using planner_client_package.Entities.Request;

namespace planner_content_service.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<JobBody?> AddAsync<T>(T jobBody, Guid accountId) where T : JobBodyRequest;
        Task<JobBody?> AddJobFromMessageAsync<T>(T jobBody, Guid accountId, Guid messageId, string snapshot) where T : JobBodyRequest;
        Task<JobBody?> GetAsync(Guid id);
        Task<bool> RemoveAsync(Guid id);
        IEnumerable<JobBody?>? GetAll(List<Guid> ids);

        Task<JobBody?> UpdateAsync(
            Guid id,
            Guid accountId,
            JobBody updatedNode,
            DateTime changeDate);
    }
}