using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.IService
{
    public interface IDraftService
    {
        Task<ServiceResponse<NodeBody>> CreateDraft(CreateOrUpdateTaskBody body, Guid accountId, Guid columnId);
        Task<ServiceResponse<IEnumerable<NodeBody>>> GetDrafts(Guid accountId, Guid boardId, Guid columnId);
        Task<ServiceResponse<NodeBody>> ConvertDraftToTask(Guid accountId, Guid boardId, Guid draftId, Guid columnId);
        Task<ServiceResponse<NodeBody>> UpdateDraft(Guid accountId, Guid draftId, CreateOrUpdateTaskBody body);
    }
}