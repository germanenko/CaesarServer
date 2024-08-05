using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, PublishEvent publishEvent);
    }
}