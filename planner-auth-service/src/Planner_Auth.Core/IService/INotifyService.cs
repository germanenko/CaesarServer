using CaesarServerLibrary.Enums;

namespace Planner_Auth.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, PublishEvent eventType);
    }
}