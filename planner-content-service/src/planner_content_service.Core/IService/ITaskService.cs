using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_server_package;

namespace planner_content_service.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<JobBody>> CreateOrUpdateJobFromMessage<T>(Guid accountId, T createOrUpdateJobBody, string snapshot, Guid messageId, CancellationToken cancellationToken) where T : JobBodyRequest;
        Task<ServiceResponse<JobBody>> CreateOrUpdateTask<T>(Guid accountId, T createOrUpdateJobBody, CancellationToken cancellationToken) where T : JobBodyRequest;
        Task<ServiceResponse<List<JobBody>>> CreateOrUpdateTasks<T>(Guid accountId, List<T> createOrUpdateTaskBodies, CancellationToken cancellationToken) where T : JobBodyRequest;
        Task<ServiceResponse<JobBody>> UpdateTask(Guid accountId, JobBody taskBody, CancellationToken cancellationToken);
        Task<ServiceResponse<List<JobBody>>> UpdateTasks(Guid accountId, List<JobBody> taskBodies, CancellationToken cancellationToken);
        System.Threading.Tasks.Task SetMessageEdited(Guid messageId, MessageState state, CancellationToken cancellationToken);
        Task<ServiceResponse<AttachedMessageBody>> AttachMessage(Guid accountId, Guid jobId, Guid messageId, string snapshot, CancellationToken cancellationToken);
        Task<ServiceResponse<ReadStateBody>> ReadAttachedMessages(Guid accountId, Guid jobId, CancellationToken cancellationToken);
    }
}