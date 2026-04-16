using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_server_package;
using JobBody = planner_client_package.Entities.JobBody;
using JobRequestBody = planner_client_package.Entities.Request.JobBody;

namespace planner_content_service.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<JobBody>> CreateTaskFromMessage<T>(Guid accountId, T createOrUpdateJobBody, string snapshot, Guid messageId) where T : JobRequestBody;
        Task<ServiceResponse<JobBody>> CreateOrUpdateTask<T>(Guid accountId, T createOrUpdateJobBody) where T : JobRequestBody;
        Task<ServiceResponse<List<JobBody>>> CreateOrUpdateTasks<T>(Guid accountId, List<T> createOrUpdateTaskBodies) where T : JobRequestBody;
        Task<ServiceResponse<JobBody>> UpdateTask(Guid accountId, JobBody taskBody);
        Task<ServiceResponse<List<JobBody>>> UpdateTasks(Guid accountId, List<JobBody> taskBodies);
    }
}