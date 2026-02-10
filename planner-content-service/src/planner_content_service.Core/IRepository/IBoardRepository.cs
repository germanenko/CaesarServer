using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<BoardBody?> AddAsync(BoardBody createBoardBody, Guid accountId);
        Task<List<BoardBody>?> AddRangeAsync(List<BoardBody> boards, Guid accountId);
        //Task<Board?> GetAsync(Guid id);
        Task<Node?> GetBoardColumn(Guid columnId);
        Task<Column?> AddBoardColumn(ColumnBody column, Guid accountId);
        Task<List<Column>?> AddBoardColumns(List<ColumnBody> columns, Guid accountId);
        Task<Node?> GetBoard(Guid boardId);
    }
}