using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Planer_task_board.Core.Entities.Events;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using Planer_task_board.Infrastructure.Data;

namespace Planer_task_board.Infrastructure.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ContentDbContext _context;
        private readonly INotifyService _notifyService;

        public TaskRepository(ContentDbContext context, INotifyService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        public async Task<TaskModel?> AddAsync
        (
            string title,
            string description,
            int priorityOrder,
            TaskState taskState,
            TaskType taskType,
            DateTime? startDate,
            DateTime? endDate,
            string? hexColor,
            BoardColumn column,
            Guid creatorId,
            List<Guid> messages
        )
        {
            var task = new TaskModel
            {
                Title = title,
                Description = description,
                HexColor = hexColor,
                PriorityOrder = priorityOrder,
                Status = taskState.ToString(),
                CreatorId = creatorId,
                IsDraft = false,
                StartDate = startDate?.ToUniversalTime(),
                EndDate = endDate?.ToUniversalTime(),
                Type = taskType.ToString(),
            };

            var setMessages = messages.Distinct().ToList();
            var taskAttachedMessages = setMessages.Select(messageId => new TaskAttachedMessage
            {
                Task = task,
                MessageId = messageId
            });


            return await AddTaskAsync(task, column, taskAttachedMessages);
        }

        public async Task<TaskModel?> AddAsync
        (
            string title,
            string description,
            string? hexColor,
            TaskType taskType,
            DateTime? startDate,
            DateTime? endDate,
            BoardColumn column,
            Guid creatorId,
            List<Guid> messages,
            TaskModel? parentTask
        )
        {
            var task = new TaskModel
            {
                Title = title,
                Description = description,
                HexColor = hexColor,
                PriorityOrder = 0,
                Status = TaskState.Undefined.ToString(),
                CreatorId = creatorId,
                IsDraft = true,
                StartDate = startDate?.ToUniversalTime(),
                EndDate = endDate?.ToUniversalTime(),
                DraftOfTask = parentTask,
                Type = taskType.ToString(),
            };

            var setMessages = messages.Distinct().ToList();
            var taskAttachedMessages = setMessages.Select(messageId => new TaskAttachedMessage
            {
                MessageId = messageId,
                Task = task
            });

            return await AddTaskAsync(task, column, taskAttachedMessages);
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, bool isDraft = false)
        {
            var result = await _context.BoardColumnTasks
                .Include(e => e.Task)
                    .ThenInclude(e => e.AttachedMessages)
                .Where(e => e.ColumnId == columnId && e.Task.IsDraft == isDraft).ToListAsync();

            return result.Select(e => e.Task);
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, TaskState? status, bool isDraft = false)
        {
            if (status == null)
                return await GetAll(columnId, isDraft);

            var statusString = status.ToString();
            var tasks = await _context.BoardColumnTasks
                .Include(e => e.Task)
                    .ThenInclude(e => e.AttachedMessages)
                .Where(e => e.ColumnId == columnId && e.Task.IsDraft == isDraft && e.Task.Status == statusString)
                .ToListAsync();

            var result = tasks.Select(e => e.Task);

            return result;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId)
        {
            var result = await _context.BoardColumnTasks
                .Include(e => e.Task)
                    .ThenInclude(e => e.AttachedMessages)
                .Where(e => e.ColumnId == columnId).ToListAsync();

            return result.Select(e => e.Task);
        }

        public async Task<TaskModel?> GetAsync(Guid id, bool isDraft)
            => await _context.Tasks
                .Include(e => e.AttachedMessages)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsDraft == isDraft);

        public async Task<bool> RemoveAsync(Guid id, bool isDraft)
        {
            var task = await GetAsync(id, isDraft);
            if (task == null)
                return true;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskModel?> ConvertDraftToTask(Guid id, Guid accountId, BoardColumn column)
        {
            var boardColumnTask = await _context.BoardColumnTasks
                .Include(e => e.Task)
                    .ThenInclude(e => e.AttachedMessages)
                .Include(e => e.Task)
                    .ThenInclude(e => e.DraftOfTask)
                .FirstOrDefaultAsync(e =>
                    e.TaskId == id && !e.Task.IsDraft && e.ColumnId == column.Id
                );

            if (boardColumnTask == null)
                return null;

            var newTask = boardColumnTask.Task.DraftOfTask;
            bool hasParentTask = newTask.DraftOfTask != null;
            var oldTask = boardColumnTask.Task;

            if (hasParentTask)
            {
                newTask.Description = oldTask.Description;
                newTask.Title = oldTask.Title;
                newTask.HexColor = oldTask.HexColor;
                newTask.PriorityOrder = oldTask.PriorityOrder;
                newTask.Status = oldTask.Status;
                newTask.StartDate = oldTask.StartDate?.ToUniversalTime();
                newTask.EndDate = oldTask.EndDate?.ToUniversalTime();
                newTask.CreatorId = accountId;
                newTask.IsDraft = false;
                newTask.Type = oldTask.Type;
            }
            else
            {
                newTask = new TaskModel
                {
                    Description = oldTask.Description,
                    Title = oldTask.Title,
                    HexColor = oldTask.HexColor,
                    PriorityOrder = oldTask.PriorityOrder,
                    Status = oldTask.Status,
                    StartDate = oldTask.StartDate?.ToUniversalTime(),
                    EndDate = oldTask.EndDate?.ToUniversalTime(),
                    CreatorId = accountId,
                    IsDraft = false,
                    Type = oldTask.Type
                };
                newTask = await AddTaskAsync(newTask, column, oldTask.AttachedMessages);
            }

            _context.Tasks.Remove(oldTask);

            await _context.SaveChangesAsync();
            return newTask;
        }

        public async Task<TaskModel?> UpdateAsync(
            Guid id,
            string title,
            string description,
            int priorityOrder,
            TaskState taskState,
            DateTime? startDate,
            DateTime? endDate,
            string? hexColor)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDraft);
            if (task == null)
                return null;

            task.Title = title;
            task.Description = description;
            task.HexColor = hexColor;
            task.PriorityOrder = priorityOrder;
            task.Status = taskState.ToString();
            task.StartDate = startDate?.ToUniversalTime();
            task.EndDate = endDate;

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskModel?> UpdateAsync(
            Guid id,
            string title,
            string description,
            DateTime? startDate,
            DateTime? endDate,
            string? hexColor,
            TaskModel? draftOfTask)
        {
            var draft = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id && e.IsDraft);
            if (draft == null)
                return null;

            draft.Title = title;
            draft.Description = description;
            draft.HexColor = hexColor;
            draft.StartDate = startDate?.ToUniversalTime();
            draft.EndDate = endDate;
            draft.IsDraft = false;
            draft.DraftOfTask = draft;

            await _context.SaveChangesAsync();
            return draft;
        }


        private async Task<TaskModel?> AddTaskAsync(
            TaskModel task,
            BoardColumn column,
            IEnumerable<TaskAttachedMessage> taskAttachedMessages)
        {
            if (task == null)
                return null;

            task.AttachedMessages = taskAttachedMessages.ToList();

            var boardColumnTask = new BoardColumnTask
            {
                Column = column,
                Task = task
            };
            task = (await _context.Tasks.AddAsync(task))?.Entity;
            boardColumnTask = (await _context.BoardColumnTasks.AddAsync(boardColumnTask))?.Entity;
            await _context.SaveChangesAsync();

            var createTaskChatEvent = new CreateTaskChatEvent
            {
                IsSuccess = false,
                CreateTaskChat = new CreateTaskChat
                {
                    TaskId = task.Id,
                    CreatorId = task.CreatorId,
                    ChatName = $"{task.Title} chat"
                }
            };

            _notifyService.Publish(createTaskChatEvent, PublishEvent.CreateTaskChatResponse);

            var boardMembers = await _context.BoardMembers.Where(e => e.BoardId == column.BoardId)
                .ToListAsync();

            var addAccountToTaskChatsEvent = new AddAccountsToTaskChatsEvent
            {
                AccountIds = boardMembers.Select(e => e.AccountId).ToList(),
                TaskIds = new List<Guid> { task.Id },
            };

            _notifyService.Publish(addAccountToTaskChatsEvent, PublishEvent.AddAccountsToTaskChats);

            return task;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, Guid userId)
        {
            var result = await _context.BoardColumnTasks
                .Include(e => e.Task)
                    .ThenInclude(e => e.AttachedMessages)
                .Where(e => e.ColumnId == columnId && e.Task.IsDraft && e.Task.CreatorId == userId)
                .ToListAsync();

            return result.Select(e => e.Task);
        }

        public async Task<TaskPerformer?> LinkPerformerToTaskAsync(TaskModel task, Guid accountId)
        {
            TaskPerformer? taskPerformer = await GetTaskPerformer(accountId, task.Id);
            if (taskPerformer != null)
                return null;

            taskPerformer = new TaskPerformer
            {
                PerformerId = accountId,
                Task = task
            };

            await _context.TaskPerformers.AddAsync(taskPerformer);
            await _context.SaveChangesAsync();

            return taskPerformer;
        }

        public async Task<TaskPerformer?> GetTaskPerformer(Guid performerId, Guid taskId)
        {
            return await _context.TaskPerformers
                .FirstOrDefaultAsync(e => e.PerformerId == performerId && e.TaskId == taskId);
        }

        public async Task<IEnumerable<TaskPerformer>> LinkPerformersToTaskAsync(TaskModel task, IEnumerable<Guid> performerIds)
        {
            var newTaskPerformers = performerIds.Select(e => new TaskPerformer
            {
                PerformerId = e,
                Task = task
            });

            await _context.TaskPerformers.AddRangeAsync(newTaskPerformers);
            await _context.SaveChangesAsync();

            return newTaskPerformers;
        }

        public async Task<IEnumerable<TaskPerformer>> GetTaskPerformers(IEnumerable<Guid> performerIds, Guid taskId, int count, int offset)
        {
            return await _context.TaskPerformers
                .Where(e => e.TaskId == taskId && performerIds.Contains(e.PerformerId))
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskPerformer>> GetTaskPerformers(Guid taskId, int count, int offset)
        {
            return await _context.TaskPerformers
                .Where(e => e.TaskId == taskId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task AssignTaskToColumn(TaskModel task, BoardColumn column)
        {
            var columnTask = await _context.BoardColumnTasks
                .FirstOrDefaultAsync(e => e.ColumnId == column.Id && e.TaskId == task.Id);

            if (columnTask != null)
                return;

            columnTask = new BoardColumnTask
            {
                Column = column,
                Task = task
            };
            await _context.BoardColumnTasks.AddAsync(columnTask);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveTaskFromColumn(Guid taskId, Guid columnId)
        {
            var columnTask = await _context.BoardColumnTasks
                .FirstOrDefaultAsync(e => e.ColumnId == columnId && e.TaskId == taskId);

            if (columnTask != null)
            {
                _context.BoardColumnTasks.Remove(columnTask);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<TaskModel?> UpdateTaskChatId(Guid taskId, Guid chatId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == taskId);

            if (task == null)
                return null;

            task.ChatId = chatId;
            await _context.SaveChangesAsync();
            return task;
        }
    }
}