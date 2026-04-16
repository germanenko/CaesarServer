using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IRepository
{
    public interface IBoardRepository
    {
        Task<BoardBody?> GetBoardById(Guid boardId);
        Task<ColumnBody?> GetColumnById(Guid columnId);
        Task<Guid?> GetUserTaskColumn(Guid accountId, Guid columnId, Guid? chatId);
        Task<List<ColumnBody>> GetUserTaskColumns(Guid accountId, Guid? chatId);
        Task<BoardBody?> CreateOrUpdateBoardAsync(BoardBody boardBody, Guid accountId, NodeBody metadata);
        Task<List<BoardBody>?> CreateOrUpdateBoards(List<BoardBody> boards, Guid accountId);
        Task<ColumnBody?> CreateOrUpdateColumn(ColumnBody column, Guid accountId, NodeBody metadata);
        Task<List<ColumnBody>?> CreateOtUpdateColumns(List<ColumnBody> columns, Guid accountId);
        Task<bool> DeleteNode(Guid nodeId, Guid accountId);
        Task<Guid> AddTaskColumn(Guid accountId, Guid columnId, Guid? chatId = null);
        System.Threading.Tasks.Task SetMessageEdited(Guid messageId, MessageState state);
    }
}