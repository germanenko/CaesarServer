using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_server_package;

namespace planner_content_service.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<BoardBody?> GetBoardById(Guid boardId, CancellationToken cancellationToken);
        Task<ColumnBody?> GetColumnById(Guid columnId, CancellationToken cancellationToken);
        Task<TaskColumnBody?> GetUserTaskColumn(Guid accountId, Guid columnId, Guid? chatId, CancellationToken cancellationToken);
        Task<List<TaskColumnBody>> GetUserTaskColumns(Guid accountId, Guid? chatId, CancellationToken cancellationToken);
        Task<BoardBody?> CreateOrUpdateBoardAsync(BoardBody boardBody, Guid accountId, NodeBody metadata, CancellationToken cancellationToken);
        Task<List<BoardBody>> CreateOrUpdateBoards(List<BoardBody> boards, Guid accountId, CancellationToken cancellationToken);
        Task<ColumnBody?> CreateOrUpdateColumn(ColumnBody column, Guid accountId, NodeBody metadata, CancellationToken cancellationToken);
        Task<List<ColumnBody>> CreateOrUpdateColumns(List<ColumnBody> columns, Guid accountId, CancellationToken cancellationToken);
        Task<bool> DeleteNode(Guid nodeId, Guid accountId, CancellationToken cancellationToken);
        Task<TaskColumnBody> AddTaskColumn(Guid accountId, TaskColumnRequest taskColumn, CancellationToken cancellationToken);
    }
}