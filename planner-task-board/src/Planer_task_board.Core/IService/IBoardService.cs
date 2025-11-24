using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
using System.Net;

namespace Planer_task_board.Core.IService
{
    public interface IBoardService
    {
        Task<ServiceResponse<BoardBody>> CreateBoardAsync(CreateBoardBody body, Guid accountId);
        Task<ServiceResponse<List<BoardBody>>> CreateBoardsAsync(List<CreateBoardBody> bodies, Guid accountId);
        Task<ServiceResponse<IEnumerable<Node>>> GetBoardsAsync(Guid accountId);
        Task<ServiceResponse<IEnumerable<BoardColumnBody>>> GetBoardColumnsAsync(Guid boardId);
        Task<ServiceResponse<IEnumerable<BoardColumnBody>>> GetAllBoardColumnsAsync(Guid boardId);
        Task<ServiceResponse<IEnumerable<Guid>>> GetBoardMembersAsync(Guid boardId, int count, int offset);
        Task<HttpStatusCode> AddBoardMemberAsync(Guid boardId, Guid accountId, Guid newAccountId, AccessType accessType);
        Task<ServiceResponse<BoardColumnBody>> AddColumn(Guid accountId, CreateColumnBody column);
        Task<ServiceResponse<List<BoardColumnBody>>> AddColumns(Guid accountId, List<CreateColumnBody> columns);
    }
}