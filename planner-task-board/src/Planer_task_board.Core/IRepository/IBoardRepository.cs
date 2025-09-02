using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;

namespace Planer_task_board.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<IEnumerable<Board>> GetAll(Guid accountId);
        Task<Board?> AddAsync(CreateBoardBody createBoardBody, Guid accountId);
        Task<List<Board>?> AddRangeAsync(List<CreateBoardBody> boards, Guid accountId);
        Task<Board?> GetAsync(Guid id);
        Task<BoardMember?> GetBoardMemberAsync(Guid accountId, Guid boardId);
        Task<BoardColumnMember?> GetColumnMemberAsync(Guid accountId, Guid columnId);
        Task<IEnumerable<BoardColumn>> GetBoardColumns(Guid boardId);
        Task<IEnumerable<BoardColumn>> GetAllBoardColumns(Guid accountId);
        Task<BoardColumn?> GetBoardColumn(Guid columnId);
        Task<IEnumerable<Guid>> GetBoardMembers(Guid boardId, int count, int offset);
        Task<IEnumerable<BoardMember>> GetBoardMembers(IEnumerable<Guid> memberIds, Guid boardId);
        Task<BoardMember?> AddBoardMember(Guid accountId, Guid boardId);
        Task<BoardColumn?> AddBoardColumn(Board board, CreateColumnBody column, Guid accountId);
        Task<List<BoardColumn>?> AddBoardColumns(List<CreateColumnBody> columns, Guid accountId);
    }
}