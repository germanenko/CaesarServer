using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_server_package;

namespace planner_content_service.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<TaskBody>> CreateTaskFromMessage(Guid accountId, Guid messageId, Guid columnId, string taskName);
        Task<ServiceResponse<TaskBody>> CreateOrUpdateTask(Guid accountId, CreateOrUpdateJobBody createOrUpdateTaskBody);
        Task<ServiceResponse<List<TaskBody>>> CreateOrUpdateTasks(Guid accountId, List<CreateOrUpdateJobBody> createOrUpdateTaskBodies);
        Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, TaskBody taskBody);
        Task<ServiceResponse<List<TaskBody>>> UpdateTasks(Guid accountId, List<TaskBody> taskBodies);
    }
}