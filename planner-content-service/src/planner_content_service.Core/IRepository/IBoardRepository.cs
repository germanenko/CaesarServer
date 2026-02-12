using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<BoardBody?> AddOrUpdateBoardAsync(BoardBody createBoardBody, Guid accountId);
        Task<List<BoardBody>?> AddRangeAsync(List<BoardBody> boards, Guid accountId);
        Task<ColumnBody?> AddBoardColumn(ColumnBody column, Guid accountId);
        Task<List<ColumnBody>?> AddBoardColumns(List<ColumnBody> columns, Guid accountId);
    }
}