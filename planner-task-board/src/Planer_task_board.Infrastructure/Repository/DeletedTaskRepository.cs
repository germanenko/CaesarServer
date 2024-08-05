using Microsoft.EntityFrameworkCore;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Infrastructure.Data;

namespace Planer_task_board.Infrastructure.Repository
{
    public class DeletedTaskRepository : IDeletedTaskRepository
    {
        private readonly ContentDbContext _context;

        public DeletedTaskRepository(ContentDbContext context)
        {
            _context = context;
        }

        public async Task<DeletedTask?> AddAsync(TaskModel deletedTask)
        {
            var task = await GetByTaskId(deletedTask.Id);
            if (task != null)
                return null;

            task = new DeletedTask
            {
                Task = deletedTask,
                ExistBeforeDate = DateTime.UtcNow.AddDays(7),
            };

            task = (await _context.DeletedTasks.AddAsync(task))?.Entity;
            deletedTask.Status = TaskState.Deleted.ToString();
            await _context.SaveChangesAsync();

            return task;
        }

        public async Task<IEnumerable<DeletedTask>> GetAll()
            => await _context.DeletedTasks
                .Include(e => e.Task)
                .ToListAsync();

        public async Task<DeletedTask?> GetByTaskId(Guid taskId)
            => await _context.DeletedTasks
                .FirstOrDefaultAsync(e => e.TaskId == taskId);

        public async Task<bool> RemoveAsync(Guid id)
        {
            var deletedTask = await _context.DeletedTasks
                .Include(e => e.Task)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (deletedTask == null)
                return true;

            _context.DeletedTasks.Remove(deletedTask);
            deletedTask.Task.Status = TaskState.Undefined.ToString();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}