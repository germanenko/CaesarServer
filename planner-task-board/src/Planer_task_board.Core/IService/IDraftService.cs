using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.IService
{
    public interface IDraftService
    {
        Task<ServiceResponse<TaskBody>> CreateDraft(CreateDraftBody body, Guid accountId, Guid columnId);
        Task<ServiceResponse<IEnumerable<TaskBody>>> GetDrafts(Guid accountId, Guid boardId, Guid columnId);
        Task<ServiceResponse<TaskBody>> ConvertDraftToTask(Guid accountId, Guid boardId, Guid draftId, Guid columnId);
        Task<ServiceResponse<TaskBody>> UpdateDraft(Guid accountId, Guid boardId, Guid draftId, UpdateDraftBody body);
    }
}