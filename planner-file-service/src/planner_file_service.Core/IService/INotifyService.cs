using planner_file_service.Core.Enums;

namespace planner_file_service.Core.IService
{
    public interface INotifyService
    {
        void Publish<T>(T message, ContentUploaded content);
    }
}