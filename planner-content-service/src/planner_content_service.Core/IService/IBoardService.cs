using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using System.Net;

namespace planner_content_service.Core.IService
{
    public interface IBoardService
    {
        Task<ServiceResponse<BoardBody>> CreateBoardAsync(BoardBody body, Guid accountId);
        Task<ServiceResponse<List<BoardBody>>> CreateBoardsAsync(List<BoardBody> bodies, Guid accountId);
        //Task<ServiceResponse<IEnumerable<BoardBody>>> GetBoardsAsync(Guid accountId);
        //Task<ServiceResponse<IEnumerable<BoardColumnBody>>> GetBoardColumnsAsync(Guid boardId);
        //Task<ServiceResponse<IEnumerable<BoardColumnBody>>> GetAllBoardColumnsAsync(Guid boardId);
        Task<ServiceResponse<IEnumerable<Guid>>> GetBoardMembersAsync(Guid boardId, int count, int offset);
        Task<HttpStatusCode> AddBoardMemberAsync(Guid boardId, Guid accountId, Guid newAccountId, AccessType accessType);
        Task<ServiceResponse<ColumnBody>> AddColumn(Guid accountId, ColumnBody column);
        Task<ServiceResponse<List<ColumnBody>>> AddColumns(Guid accountId, List<ColumnBody> columns);
    }
}