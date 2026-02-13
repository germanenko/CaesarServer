using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<BoardBody?> CreateOrUpdateBoardAsync(BoardBody createBoardBody, Guid accountId);
        Task<List<BoardBody>?> CreateOrUpdateBoards(List<BoardBody> boards, Guid accountId);
        Task<ColumnBody?> CreateOrUpdateColumn(ColumnBody column, Guid accountId);
        Task<List<ColumnBody>?> CreateOtUpdateColumns(List<ColumnBody> columns, Guid accountId);
    }
}