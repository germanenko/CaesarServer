using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.IService
{
    public interface IDraftService
    {
        Task<ServiceResponse<Node>> CreateDraft(Node body, Guid accountId, Guid columnId);
        Task<ServiceResponse<IEnumerable<Node>>> GetDrafts(Guid accountId, Guid boardId, Guid columnId);
        Task<ServiceResponse<Node>> ConvertDraftToTask(Guid accountId, Guid boardId, Guid draftId, Guid columnId);
        Task<ServiceResponse<Node>> UpdateDraft(Guid accountId, Guid boardId, Guid draftId, Node body);
    }
}