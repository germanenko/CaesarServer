using planner_server_package.Entities;
using planner_server_package.Events.Enums;

namespace planner_chat_service.Core.IService
{
    public interface INotifyService
    {
        Task<ServiceResponse<TResult>> Publish<T, TResult>(T message, PublishEvent eventType);
        void Publish<T>(T message, PublishEvent eventType);
    }
}