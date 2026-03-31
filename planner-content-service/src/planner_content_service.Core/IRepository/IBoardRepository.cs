using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<BoardBody?> GetBoardById(Guid boardId);
        Task<BoardBody?> CreateOrUpdateBoardAsync(CreateOrUpdateBoardBody createBoardBody, Guid accountId);
        Task<List<BoardBody>?> CreateOrUpdateBoards(List<CreateOrUpdateBoardBody> boards, Guid accountId);
        Task<ColumnBody?> CreateOrUpdateColumn(ColumnBody column, Guid accountId);
        Task<List<ColumnBody>?> CreateOtUpdateColumns(List<ColumnBody> columns, Guid accountId);
        Task<bool> DeleteNode(Guid nodeId, Guid accountId);
    }
}