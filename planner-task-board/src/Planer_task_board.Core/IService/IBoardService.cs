using System.Net;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.IService
{
    public interface IBoardService
    {
        Task<ServiceResponse<BoardBody>> CreateBoardAsync(CreateBoardBody body, Guid accountId);
        Task<ServiceResponse<IEnumerable<BoardBody>>> GetBoardsAsync(Guid accountId);
        Task<ServiceResponse<IEnumerable<BoardColumnBody>>> GetBoardColumnsAsync(Guid boardId);
        Task<ServiceResponse<IEnumerable<Guid>>> GetBoardMembersAsync(Guid boardId, int count, int offset);
        Task<HttpStatusCode> AddBoardMemberAsync(Guid boardId, Guid accountId, Guid newAccountId);
        Task<HttpStatusCode> AddColumn(Guid accountId, Guid boardId, string name);
    }
}