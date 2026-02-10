using planner_server_package.Entities;
using planner_server_package.Events.Enums;

namespace planner_content_service.Core.IService
{
    public interface INotifyService
    {
        Task<ServiceResponse<object>> Publish<T>(T message, PublishEvent publishEvent);
    }
}