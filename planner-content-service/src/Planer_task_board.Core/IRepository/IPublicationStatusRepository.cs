using CaesarServerLibrary.Enums;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.IRepository
{
    public interface IPublicationStatusRepository
    {
        Task<PublicationStatusModel?> ChangeStatus(Guid nodeId, PublicationStatus status);
        Task<List<PublicationStatusModel>?> Get(PublicationStatus status);
    }
}