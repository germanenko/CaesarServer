using Microsoft.EntityFrameworkCore;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_content_service.Infrastructure.Data;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using System.Text.Json;

namespace planner_content_service.Infrastructure.Repository
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
            TaskBody task,
            Guid accountId
        )
        {
            var node = new TaskModel()
            {
                Id = task.Id,
                Name = task.Name,
                Type = NodeType.Task
            };

            _context.SaveChanges();

            CreateTaskEvent taskEvent = new CreateTaskEvent()
            {
                Task = BodyConverter.ClientToServerBody(task),
                CreatorId = accountId
            };

            _notifyService.Publish(taskEvent, PublishEvent.CreateTask);


            return await AddTaskAsync(node, accountId, task.PublicationStatus, task.Link);
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, bool isDraft = false)
        {
            var result = await _context.NodeLinks
                .Where(x => x.ParentId == columnId)
                .Join(_context.Tasks,
                    n => n.ChildId,
                    t => t.Id,
                    (n, t) => t)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, WorkflowStatus? status, bool isDraft = false)
        {
            if (status == null)
                return await GetAll(columnId, isDraft);

            var statusString = status.ToString();

            var result = await _context.NodeLinks
                .Where(x => x.ParentId == columnId)
                .Join(_context.WorkflowStatuses,
                    n => n.ChildId,
                    t => t.NodeId,
                    (n, t) => t)
                .Where(x => x.Status == status)
                .Join(_context.Tasks,
                    w => w.NodeId,
                    n => n.Id,
                    (w, n) => n)
                .ToListAsync();


            return result;
        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId)
        {
            var result = await _context.NodeLinks
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
                .Include(x => x.Node)
                .Where(ar => ar.AccountId == accountId && ar.Node is Board)
                .Join(_context.NodeLinks,
                    ar => ar.NodeId,
                    n1 => n1.ParentId,
                    (ar, n1) => new { ColumnId = n1.ChildId })
                .Join(_context.NodeLinks,
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
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<bool> RemoveAsync(Guid id, bool isDraft)
        {
            var task = await GetAsync(id, isDraft);
            if (task == null)
                return true;

            _context.Nodes.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskModel?> ConvertDraftToTask(Guid id, Guid accountId, Guid? columnId)
        {
            var result = await _context.NodeLinks
                .Where(x => x.ParentId == columnId && x.ChildId == id)
                .Join(_context.PublicationStatuses
                    .Include(ps => ps.Node), // Явно включаем Node
                    nl => nl.ChildId,
                    ps => ps.NodeId,
                    (nl, ps) => new { PublicationStatus = ps, Node = ps.Node })
                .FirstOrDefaultAsync(x => x.PublicationStatus.NodeId == id &&
                                 x.PublicationStatus.Status == PublicationStatus.Draft);

            if (result == null)
                return null;

            result.PublicationStatus.Status = PublicationStatus.Active;

            await _context.SaveChangesAsync();
            return result.Node as TaskModel;
        }

        public async Task<TaskModel?> UpdateAsync(
            Guid id,
            Guid accountId,
            TaskBody updatedNode,
            Guid? columnId,
            DateTime changeDate)
        {
            var status = await _context.PublicationStatuses
                .Include(ps => ps.Node)
                .FirstOrDefaultAsync(e => e.NodeId == id && e.Status == PublicationStatus.Draft);
            if (status == null)
                return null;

            var task = status.Node as TaskModel;

            task.Name = updatedNode.Name;
            task.Props = JsonSerializer.Serialize(updatedNode);

            var boardColumnTask = _context.NodeLinks.Where(x => x.ChildId == task.Id).First();
            if (boardColumnTask.ParentId != columnId)
            {
                await RemoveTaskFromColumn(task.Id, boardColumnTask.ParentId);
                await AssignTaskToColumn(task.Id, columnId.Value);
            }

            var node = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id);

            node = task;

            await _context.SaveChangesAsync();

            return node;
        }

        public async Task<TaskModel?> UpdateAsync(
            Guid id,
            Guid accountId,
            TaskBody updatedNode,
            DateTime changeDate)
        {
            var draftStatus = await _context.PublicationStatuses
                .Include(x => x.Node)
                .FirstOrDefaultAsync(e => e.NodeId == id && e.Status == PublicationStatus.Draft);
            if (draftStatus == null)
                return null;

            var draft = draftStatus.Node as TaskModel;

            draft.Name = updatedNode.Name;
            draft.Props = JsonSerializer.Serialize(updatedNode);

            await _context.SaveChangesAsync();
            return draft;
        }


        private async Task<TaskModel?> AddTaskAsync(
            TaskModel task,
            Guid accountId,
            PublicationStatus publicationStatus,
            NodeLinkBody? attach = null)
        {
            if (task == null)
                return null;

            try
            {

                task = (await _context.Tasks.AddAsync(task)).Entity;

                if (attach != null)
                {
                    _context.NodeLinks.Add(new NodeLink() { Id = attach.Id, ParentId = attach.ParentId, ChildId = attach.ChildId, RelationType = attach.RelationType });
                }

                _context.PublicationStatuses.Add(new PublicationStatusModel()
                {
                    Node = task,
                    NodeId = task.Id,
                    Status = publicationStatus
                });

                await _context.SaveChangesAsync();

                var createTaskChatEvent = new CreateTaskChatEvent
                {
                    IsSuccess = false,
                    CreateTaskChat = new CreateTaskChat
                    {
                        TaskId = task.Id,
                        CreatorId = accountId,
                        ChatName = $"{task.Name} chat"
                    }
                };

                _ = Task.Run(() => _notifyService.Publish(createTaskChatEvent, PublishEvent.CreateTaskChatResponse));


                return task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания задачи: {ex.Message}");

                throw;
            }

        }

        public async Task<IEnumerable<TaskModel>> GetAll(Guid columnId, Guid userId)
        {
            var result = await _context.NodeLinks
                .Where(e => e.ParentId == columnId)
                .Join(_context.Tasks,
                    n1 => n1.ChildId,
                    t => t.Id,
                    (n1, t) => t)
                .ToListAsync();

            return result;
        }



        public async Task AssignTaskToColumn(Guid taskId, Guid columnId)
        {
            var columnTask = await _context.NodeLinks.FirstOrDefaultAsync(e => e.ParentId == columnId && e.ChildId == taskId);

            if (columnTask != null)
                return;

            columnTask = new NodeLink
            {
                ParentId = columnId,
                ChildId = taskId,
                RelationType = RelationType.Contains
            };
            await _context.NodeLinks.AddAsync(columnTask);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveTaskFromColumn(Guid taskId, Guid columnId)
        {
            var columnTask = await _context.NodeLinks
                .FirstOrDefaultAsync(e => e.ParentId == columnId && e.ChildId == taskId);

            if (columnTask != null)
            {
                _context.NodeLinks.Remove(columnTask);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<NodeLink?> UpdateTaskChatId(Guid taskId, Guid chatId)
        {
            var task = await _context.NodeLinks
                .FirstOrDefaultAsync(e => e.ChildId == taskId && e.RelationType == RelationType.Attach);

            if (task == null)
                return null;

            task.ParentId = chatId;
            await _context.SaveChangesAsync();
            return task;
        }
    }
}