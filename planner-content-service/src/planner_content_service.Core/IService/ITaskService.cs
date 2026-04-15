using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_server_package;
using TaskBody = planner_client_package.Entities.TaskBody;

namespace planner_content_service.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<TaskBody>> CreateTaskFromMessage<T>(Guid accountId, T createOrUpdateJobBody, string snapshot, Guid messageId) where T : JobBody;
        Task<ServiceResponse<TaskBody>> CreateOrUpdateTask<T>(Guid accountId, T createOrUpdateJobBody) where T : JobBody;
        Task<ServiceResponse<List<TaskBody>>> CreateOrUpdateTasks(Guid accountId, List<JobBody> createOrUpdateTaskBodies);
        Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, TaskBody taskBody);
        Task<ServiceResponse<List<TaskBody>>> UpdateTasks(Guid accountId, List<TaskBody> taskBodies);
    }
}