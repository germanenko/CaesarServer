using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Planer_task_board.Core.Entities.Events;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Response;
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
            Guid id,
            string title,
            string description,
            int priorityOrder,
            TaskState taskState,
            TaskType taskType,
            DateTime? startDate,
            DateTime? endDate,
            string? hexColor,
            BoardColumn? column,
            Guid creatorId,
            Guid message,
            DateTime changeDate
        )
        {
            var task = new TaskModel
            {
                Id = id,
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
                UpdatedAt = changeDate
            };

            var taskAttachedMessage = new TaskAttachedMessage
            {
                Task = task,
                MessageId = message
            };


            return await AddTaskAsync(task, column, taskAttachedMessage);
        }

        public async Task<TaskModel?> AddAsync
        (
            Guid id,
            string title,
            string description,
            string? hexColor,
            TaskType taskType,
            DateTime? startDate,
            DateTime? endDate,
            BoardColumn column,
            Guid creatorId,
            Guid message,
            TaskModel? parentTask,
            DateTime changeDate
        )
        {
            var task = new TaskModel
            {
                Id = id,
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
                UpdatedAt = changeDate
            };

            var taskAttachedMessage = new TaskAttachedMessage
            {
                MessageId = message,
                Task = task
            };

            return await AddTaskAsync(task, column, taskAttachedMessage);
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, bool isDraft = false)
        {
            var result = await _context.Nodes
                .Where(x => x.ParentId == columnId)
                .Join(_context.Tasks,
                    n => n.ChildId,
                    t => t.Id,
                    (n, t) => t)
                .Where(x => x.IsDraft == isDraft)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, TaskState? status, bool isDraft = false)
        {
            if (status == null)
                return await GetAll(columnId, isDraft);

            var statusString = status.ToString();

            var result = await _context.Nodes
                .Where(x => x.ParentId == columnId)
                .Join(_context.Tasks,
                    n => n.ChildId,
                    t => t.Id,
                    (n, t) => t)
                .Where(x => x.IsDraft == isDraft && x.Status == statusString)
                .ToListAsync();


            return result;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId)
        {
            var result = await _context.Nodes
                .Where(x => x.ParentId == columnId)
                .Join(_context.Tasks,
                    n => n.ChildId,
                    t => t.Id,
                    (n, t) => t)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<TaskModel>> GetAllTasks(Guid accountId)
        {
            var tasks = await _context.AccessRights
                .Where(ar => ar.AccountId == accountId && ar.ResourceType == ResourceType.Board)
                .Join(_context.Nodes,
                    ar => ar.ResourceId,  
                    n1 => n1.ParentId,    
                    (ar, n1) => new { ColumnId = n1.ChildId })
                .Join(_context.Nodes,
                    x => x.ColumnId,      
                    n2 => n2.ParentId,    
                    (x, n2) => new { TaskId = n2.ChildId })
                .Join(_context.Tasks,
                    x => x.TaskId,        
                    t => t.Id,            
                    (x, t) => t)          
                .ToListAsync();

            return tasks;
        }

        public async Task<TaskModel?> GetAsync(Guid id, bool isDraft)
            => await _context.Tasks
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
            var boardColumnTask = await _context.Nodes
                .Where(x => x.ParentId == column.Id)
                .Join(_context.Tasks,
                    n => n.ChildId,
                    t => t.Id,
                    (x, t) => t)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDraft);

            if (boardColumnTask == null)
                return null;

            var newTask = boardColumnTask.DraftOfTask;
            bool hasParentTask = newTask.DraftOfTask != null;
            var oldTask = boardColumnTask;

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
                newTask = await AddTaskAsync(newTask, column);
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
            string? hexColor,
            Guid? columnId,
            DateTime changeDate)
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
            task.UpdatedAt = changeDate;

            var boardColumnTask = _context.Nodes.Where(x => x.ChildId == task.Id).First();
            if (boardColumnTask.ParentId != columnId)
            {
                await RemoveTaskFromColumn(task.Id, boardColumnTask.ParentId);
                var column = _context.BoardColumns.Where(x => x.Id == columnId).First();
                await AssignTaskToColumn(task, column); 
            }

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
            TaskModel? draftOfTask,
            DateTime changeDate)
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
            draft.UpdatedAt = changeDate;

            await _context.SaveChangesAsync();
            return draft;
        }


        private async Task<TaskModel?> AddTaskAsync(
            TaskModel task,
            BoardColumn? column,
            TaskAttachedMessage? taskAttachedMessage = null)
        {
            if (task == null)
                return null;

            task = (await _context.Tasks.AddAsync(task))?.Entity;

            if(column != null)
            {
                var node = new Node()
                {
                    ParentId = column.Id,
                    ChildId = task.Id,
                    RelationType = RelationType.Contains
                };

                await _context.Nodes.AddAsync(node);
            }



            if (taskAttachedMessage != null)
            {
                _context.Nodes.Add(new Node()
                {
                    ParentId = taskAttachedMessage.MessageId,
                    ChildId = task.Id,
                    RelationType = RelationType.Attach
                });
            }

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

            if (column != null)
            {
                var boardMembers = await _context.Nodes.Where(e => e.ChildId == column.Id)
                    .Join(_context.Boards,
                        n => n.ParentId,
                        b => b.Id,
                        (n, b) => b)
                    .Join(_context.AccessRights,
                        b => b.Id, 
                        a => a.ResourceId,
                        (n, a) => a)
                    .Select(a => a.AccountId)
                    .ToListAsync();


                var addAccountToTaskChatsEvent = new AddAccountsToTaskChatsEvent
                {
                    AccountIds = boardMembers.ToList(),
                    TaskIds = new List<Guid> { task.Id },
                };

                _notifyService.Publish(addAccountToTaskChatsEvent, PublishEvent.AddAccountsToTaskChats);
            }


            return task;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, Guid userId)
        {
            var result = await _context.Nodes
                .Where(e => e.ParentId == columnId)
                .Join(_context.Tasks,
                    n1 => n1.ChildId,
                    t => t.Id,
                    (n1, t) => t)
                .Where(x => x.CreatorId == userId)
                .ToListAsync();

            return result;
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
            var columnTask = await _context.Nodes.FirstOrDefaultAsync(e => e.ParentId == column.Id && e.ChildId == task.Id);

            if (columnTask != null)
                return;

            columnTask = new Node
            {
                ParentId = column.Id,
                ChildId = task.Id,
                RelationType = RelationType.Contains
            };
            await _context.Nodes.AddAsync(columnTask);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveTaskFromColumn(Guid taskId, Guid columnId)
        {
            var columnTask = await _context.Nodes
                .FirstOrDefaultAsync(e => e.ParentId == columnId && e.ChildId == taskId);

            if (columnTask != null)
            {
                _context.Nodes.Remove(columnTask);
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