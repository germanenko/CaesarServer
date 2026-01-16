using planner_server_package.Enums;

namespace planner_node_service.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, PublishEvent publishEvent);
    }
}