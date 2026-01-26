using planner_server_package.Events.Enums;

namespace planner_chat_service.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, NotifyPublishEvent eventType);
    }
}