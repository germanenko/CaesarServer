using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<IEnumerable<Node>> GetAll(Guid accountId);
        Task<Node?> AddAsync(CreateBoardBody createBoardBody, Guid accountId);
        Task<List<Node>?> AddRangeAsync(List<CreateBoardBody> boards, Guid accountId);
        //Task<Board?> GetAsync(Guid id);
        Task<AccessRight?> GetBoardMemberAsync(Guid accountId, Guid boardId);
        Task<IEnumerable<Node>> GetBoardColumns(Guid boardId);
        Task<IEnumerable<Node>> GetAllBoardColumns(Guid accountId);
        Task<Node?> GetBoardColumn(Guid columnId);
        Task<Node?> GetBoardColumnByChild(Guid childId);
        Task<IEnumerable<Guid>> GetBoardMembers(Guid boardId, int count, int offset);
        Task<IEnumerable<AccessRight>> GetBoardMembers(IEnumerable<Guid?> memberIds, Guid boardId);
        Task<AccessRight?> AddBoardMember(Guid accountId, Guid boardId, AccessType accessType);
        Task<Node?> AddBoardColumn(CreateColumnBody column, Guid accountId);
        Task<List<Node>?> AddBoardColumns(List<CreateColumnBody> columns, Guid accountId);
        Task<Node?> GetBoard(Guid columnId);
    }
}