using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface ITaskRepository
    {
        Task<JobBody?> AddAsync<T>(T jobBody, Guid accountId, CancellationToken cancellationToken) where T : JobBodyRequest;
        Task<JobBody?> AddJobFromMessageAsync<T>(T jobBody, Guid accountId, Guid messageId, string snapshot, CancellationToken cancellationToken) where T : JobBodyRequest;
        Task<JobBody?> GetAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken);
        IEnumerable<JobBody?>? GetAll(List<Guid> ids, CancellationToken cancellationToken);

        Task<JobBody?> UpdateAsync(
            Guid id,
            Guid accountId,
            JobBody updatedNode,
            DateTime changeDate,
            CancellationToken cancellationToken);

        Task<AttachedMessage> AttachMessage(Guid accountId, Guid jobId, Guid messageId, string snapshot, CancellationToken cancellationToken);
        System.Threading.Tasks.Task SetMessageEdited(Guid messageId, MessageState state, CancellationToken cancellationToken);
        Task<AttachedMessage?> GetAttachedMessage(Guid jobId, Guid messageId, CancellationToken cancellationToken);
        Task<ReadStateBody> GetOrCreateReadStateAsync(Guid accountId, Guid jobId, CancellationToken cancellationToken);
        Task<ReadStateBody> UpdateReadState(Guid accountId, Guid jobId, CancellationToken cancellationToken);
    }
}