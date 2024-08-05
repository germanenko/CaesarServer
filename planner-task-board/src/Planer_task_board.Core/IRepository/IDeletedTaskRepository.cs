using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Core.IRepository
{
    public interface IDeletedTaskRepository
    {
        Task<DeletedTask?> AddAsync(TaskModel deletedTask);
        Task<DeletedTask?> GetByTaskId(Guid taskId);
        Task<bool> RemoveAsync(Guid id);
        Task<IEnumerable<DeletedTask>> GetAll();
    }
}