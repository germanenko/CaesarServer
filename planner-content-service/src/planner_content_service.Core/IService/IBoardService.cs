using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_server_package;
using planner_server_package.Entities;
using BoardBody = planner_client_package.Entities.BoardBody;
using ColumnBody = planner_client_package.Entities.ColumnBody;

namespace planner_content_service.Core.IService
{
    public interface IBoardService
    {
        Task<ServiceResponse<BoardBody>> CreateOrUpdateBoardAsync(CreateOrUpdateBoardBody body, Guid accountId, CancellationToken cancellationToken);
        Task<ServiceResponse<List<BoardBody>>> CreateOrUpdateBoards(List<CreateOrUpdateBoardBody> bodies, Guid accountId, CancellationToken cancellationToken);
        Task<ServiceResponse<ColumnBody>> CreateOrUpdateColumn(Guid accountId, ColumnBodyRequest column, CancellationToken cancellationToken);
        Task<ServiceResponse<List<ColumnBody>>> CreateOrUpdateColumns(Guid accountId, List<ColumnBody> columns, CancellationToken cancellationToken);
        Task<ServiceResponse<bool>> DeleteNode(Guid accountId, Guid columnId, CancellationToken cancellationToken);
        Task<ServiceResponse<TaskColumnBody>> AddDefaultColumn(Guid accountId, TaskColumnRequest taskColumn, CancellationToken cancellationToken);
        Task<ServiceResponse<List<TaskColumnBody>>> GetDefaultColumns(Guid accountId, Guid? chatId, CancellationToken cancellationToken);
    }
}