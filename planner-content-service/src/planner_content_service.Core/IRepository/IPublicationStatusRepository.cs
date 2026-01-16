using planner_server_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface IPublicationStatusRepository
    {
        Task<PublicationStatusModel?> ChangeStatus(Guid nodeId, PublicationStatus status);
        Task<List<PublicationStatusModel>?> Get(PublicationStatus status);
    }
}