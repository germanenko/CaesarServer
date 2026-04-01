using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_server_package;
using planner_server_package.Entities;
using BoardBody = planner_client_package.Entities.BoardBody;
using ColumnBody = planner_client_package.Entities.ColumnBody;

namespace planner_content_service.Core.IService
{
    public interface IBoardService
    {
        Task<ServiceResponse<BoardBody>> CreateOrUpdateBoardAsync(CreateOrUpdateBoardBody body, Guid accountId);
        Task<ServiceResponse<List<BoardBody>>> CreateOrUpdateBoards(List<CreateOrUpdateBoardBody> bodies, Guid accountId);
        Task<ServiceResponse<ColumnBody>> CreateOrUpdateColumn(Guid accountId, ColumnBody column);
        Task<ServiceResponse<bool>> DeleteNode(Guid accountId, Guid columnId);
        Task<ServiceResponse<List<ColumnBody>>> CreateOrUpdateColumns(Guid accountId, List<ColumnBody> columns);
    }
}