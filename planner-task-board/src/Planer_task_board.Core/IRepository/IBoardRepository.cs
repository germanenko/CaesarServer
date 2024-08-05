using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<IEnumerable<Board>> GetAll(Guid accountId);
        Task<Board?> AddAsync(string boardName, Guid accountId);
        Task<Board?> GetAsync(Guid id);
        Task<BoardMember?> GetBoardMemberAsync(Guid accountId, Guid boardId);
        Task<IEnumerable<BoardColumn>> GetBoardColumns(Guid boardId);
        Task<BoardColumn?> GetBoardColumn(Guid columnId);
        Task<IEnumerable<Guid>> GetBoardMembers(Guid boardId, int count, int offset);
        Task<IEnumerable<BoardMember>> GetBoardMembers(IEnumerable<Guid> memberIds, Guid boardId);
        Task<BoardMember?> AddBoardMember(Guid accountId, Guid boardId);
        Task<BoardColumn?> AddBoardColumn(Board board, string name);
    }
}