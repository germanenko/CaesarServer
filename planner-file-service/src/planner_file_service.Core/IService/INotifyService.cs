using planner_file_service.Core.Enums;
using planner_server_package.Entities;

namespace planner_file_service.Core.IService
{
    public interface INotifyService
    {
        Task<ServiceResponse<object>> Publish<T>(T message, ContentUploaded content);
    }
}