using planner_client_package.Entities;
using planner_common_package.Enums;
using System.Net;

namespace planner_content_service.Core.IService
{
    public interface ITaskService
    {
        Task<ServiceResponse<TaskBody>> CreateOrUpdateTask(Guid accountId, TaskBody taskBody);
        Task<ServiceResponse<List<TaskBody>>> CreateOrUpdateTasks(Guid accountId, List<TaskBody> taskBodies);
        Task<ServiceResponse<TaskBody>> UpdateTask(Guid accountId, TaskBody taskBody);
        Task<ServiceResponse<List<TaskBody>>> UpdateTasks(Guid accountId, List<TaskBody> taskBodies);
    }
}