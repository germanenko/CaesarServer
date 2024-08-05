using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, NotifyPublishEvent eventType);
    }
}