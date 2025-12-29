using CaesarServerLibrary.Enums;

namespace Planer_task_board.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, PublishEvent publishEvent);
        INotifyService Initialize();
    }
}