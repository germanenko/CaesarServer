using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

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

        Task<AttachedMessage> AttachMessage(Guid accountId, Guid jobId, Guid messageId, string snapshot);
        System.Threading.Tasks.Task SetMessageEdited(Guid messageId, MessageState state);
        Task<AttachedMessage?> GetAttachedMessage(Guid jobId, Guid messageId);
        Task<ReadStateBody> GetOrCreateReadStateAsync(Guid accountId, Guid jobId);
        Task<ReadStateBody> UpdateReadState(Guid accountId, Guid jobId);
    }
}