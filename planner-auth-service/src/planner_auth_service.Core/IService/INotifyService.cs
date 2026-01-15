using CaesarServerLibrary.Enums;

namespace planner_auth_service.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, PublishEvent eventType);
    }
}