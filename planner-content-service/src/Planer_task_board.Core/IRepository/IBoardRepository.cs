using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<IEnumerable<Node>> GetAll(Guid accountId);
        Task<Board?> AddAsync(BoardBody createBoardBody, Guid accountId);
        Task<List<Board>?> AddRangeAsync(List<BoardBody> boards, Guid accountId);
        //Task<Board?> GetAsync(Guid id);
        Task<AccessRight?> GetBoardMemberAsync(Guid accountId, Guid boardId);
        Task<IEnumerable<Node>> GetBoardColumns(Guid boardId);
        Task<IEnumerable<Node>> GetAllBoardColumns(Guid accountId);
        Task<Node?> GetBoardColumn(Guid columnId);
        Task<Node?> GetBoardColumnByChild(Guid childId);
        Task<IEnumerable<Guid>> GetBoardMembers(Guid boardId, int count, int offset);
        Task<IEnumerable<AccessRight>> GetBoardMembers(IEnumerable<Guid?> memberIds, Guid boardId);
        Task<AccessRight?> AddBoardMember(Guid accountId, Guid boardId, AccessType accessType);
        Task<Column?> AddBoardColumn(ColumnBody column, Guid accountId);
        Task<List<Column>?> AddBoardColumns(List<ColumnBody> columns, Guid accountId);
        Task<Node?> GetBoard(Guid columnId);
    }
}