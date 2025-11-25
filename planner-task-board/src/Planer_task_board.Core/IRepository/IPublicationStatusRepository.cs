using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface IPublicationStatusRepository
    {
        Task<PublicationStatusModel?> ChangeStatus(Guid nodeId, PublicationStatus status);
        Task<List<PublicationStatusModel>?> Get(PublicationStatus status);
    }
}