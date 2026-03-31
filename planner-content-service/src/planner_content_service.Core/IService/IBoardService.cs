using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using System.Net;

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