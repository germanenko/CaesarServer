using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<IEnumerable<Board>> GetAll(Guid accountId);
        Task<Board?> AddAsync(CreateBoardBody createBoardBody, Guid accountId);
        Task<List<Board>?> AddRangeAsync(List<CreateBoardBody> boards, Guid accountId);
        Task<Board?> GetAsync(Guid id);
        Task<AccessRight?> GetBoardMemberAsync(Guid accountId, Guid boardId);
        Task<IEnumerable<BoardColumn>> GetBoardColumns(Guid boardId);
        Task<IEnumerable<BoardColumn>> GetAllBoardColumns(Guid accountId);
        Task<BoardColumn?> GetBoardColumn(Guid? columnId);
        Task<IEnumerable<Guid>> GetBoardMembers(Guid boardId, int count, int offset);
        Task<IEnumerable<AccessRight>> GetBoardMembers(IEnumerable<Guid> memberIds, Guid boardId);
        Task<AccessRight?> AddBoardMember(Guid accountId, Guid boardId, AccessType accessType);
        Task<BoardColumn?> AddBoardColumn(CreateColumnBody column, Guid accountId);
        Task<List<BoardColumn>?> AddBoardColumns(List<CreateColumnBody> columns, Guid accountId);
        Task<Board?> GetBoard(Guid columnId);
    }
}