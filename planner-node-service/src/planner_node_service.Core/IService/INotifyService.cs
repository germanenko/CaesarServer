using planner_server_package.Events.Enums;

namespace planner_node_service.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, PublishEvent publishEvent);
    }
}