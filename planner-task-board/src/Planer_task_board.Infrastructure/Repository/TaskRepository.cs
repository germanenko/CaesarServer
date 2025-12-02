using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Planer_task_board.Core.Entities.Events;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using Planer_task_board.Infrastructure.Data;
using System.Text.Json;
using System.Threading.Tasks;

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

        public async Task<Node?> AddAsync
        (
            CreateOrUpdateTaskBody task,
            Guid accountId
        )
        {
            var props = JsonSerializer.Serialize(task);

            var node = new Node()
            {
                Id = task.Id,
                Name = task.Title,
                Props = props,
                Type = NodeType.Task
            };

            await _context.History.AddAsync(new History
            {
                NodeId = node.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = accountId
            });

            var taskAttachedMessage = new TaskAttachedMessage
            {
                MessageId = task.MessageId,
                Task = node
            };

            return await AddTaskAsync(node, accountId, task.ColumnId, task.PublicationStatus, taskAttachedMessage);
        }

        public async Task<IEnumerable<Node>> GetAll(Guid columnId, bool isDraft = false)
        {
            var result = await _context.NodeLinks
                .Where(x => x.ParentId == columnId)
                .Join(_context.Nodes,
                    n => n.ChildId,
                    t => t.Id,
                    (n, t) => t)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<Node>> GetAll(Guid columnId, WorkflowStatus? status, bool isDraft = false)
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
                .Join(_context.Nodes,
                    w => w.NodeId,
                    n => n.Id,
                    (w, n) => n)
                .ToListAsync();


            return result;
        }

        public async Task<IEnumerable<Node>> GetAll(Guid columnId)
        {
            var result = await _context.NodeLinks
                .Where(x => x.ParentId == columnId)
                .Join(_context.Nodes,
                    n => n.ChildId,
                    t => t.Id,
                    (n, t) => t)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<Node>> GetAllTasks(Guid accountId)
        {
            var tasks = await _context.AccessRights
                .Where(ar => ar.AccountId == accountId && ar.ResourceType == NodeType.Board)
                .Join(_context.NodeLinks,
                    ar => ar.NodeId,  
                    n1 => n1.ParentId,    
                    (ar, n1) => new { ColumnId = n1.ChildId })
                .Join(_context.NodeLinks,
                    x => x.ColumnId,      
                    n2 => n2.ParentId,    
                    (x, n2) => new { TaskId = n2.ChildId })
                .Join(_context.Nodes,
                    x => x.TaskId,        
                    t => t.Id,            
                    (x, t) => t)          
                .ToListAsync();

            return tasks;
        }

        public async Task<Node?> GetAsync(Guid id, bool isDraft)
            => await _context.Nodes
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

        public async Task<Node?> ConvertDraftToTask(Guid id, Guid accountId, Guid? columnId)
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

            //var newTask = boardColumnTask.DraftOfTask;
            //bool hasParentTask = newTask.DraftOfTask != null;
            //var oldTask = boardColumnTask;

            //if (hasParentTask)
            //{
            //    newTask.Description = oldTask.Description;
            //    newTask.Title = oldTask.Title;
            //    newTask.HexColor = oldTask.HexColor;
            //    newTask.PriorityOrder = oldTask.PriorityOrder;
            //    newTask.Status = oldTask.Status;
            //    newTask.StartDate = oldTask.StartDate?.ToUniversalTime();
            //    newTask.EndDate = oldTask.EndDate?.ToUniversalTime();
            //    newTask.CreatorId = accountId;
            //    newTask.IsDraft = false;
            //    newTask.Type = oldTask.Type;
            //}
            //else
            //{
            //    newTask = new TaskModel
            //    {
            //        Description = oldTask.Description,
            //        Title = oldTask.Title,
            //        HexColor = oldTask.HexColor,
            //        PriorityOrder = oldTask.PriorityOrder,
            //        Status = oldTask.Status,
            //        StartDate = oldTask.StartDate?.ToUniversalTime(),
            //        EndDate = oldTask.EndDate?.ToUniversalTime(),
            //        CreatorId = accountId,
            //        IsDraft = false,
            //        Type = oldTask.Type
            //    };
            //    newTask = await AddTaskAsync(newTask, columnId);
            //}

            //_context.Tasks.Remove(oldTask);

            result.PublicationStatus.Status = PublicationStatus.Active;

            await _context.SaveChangesAsync();
            return result.Node;
        }

        public async Task<Node?> UpdateAsync(
            Guid id,
            Guid accountId,
            CreateOrUpdateTaskBody updatedNode,
            Guid? columnId,
            DateTime changeDate)
        {
            var status = await _context.PublicationStatuses
                .Include(ps => ps.Node)
                .FirstOrDefaultAsync(e => e.NodeId == id && e.Status == PublicationStatus.Draft);
            if (status == null)
                return null;

            var task = status.Node;

            task.Name = updatedNode.Title;
            task.Props = JsonSerializer.Serialize(updatedNode); 

            await _context.History.AddAsync(new History
            {
                NodeId = task.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = accountId
            });

            var boardColumnTask = _context.NodeLinks.Where(x => x.ChildId == task.Id).First();
            if (boardColumnTask.ParentId != columnId)
            {
                await RemoveTaskFromColumn(task.Id, boardColumnTask.ParentId);
                await AssignTaskToColumn(task.Id, columnId.Value); 
            }

            var node = await _context.Nodes
                .FirstOrDefaultAsync(e => e.Id == id);

            node = task;

            await _context.SaveChangesAsync();

            return node;
        }

        public async Task<Node?> UpdateAsync(
            Guid id,
            Guid accountId,
            CreateOrUpdateTaskBody updatedNode,
            DateTime changeDate)
        {
            var draftStatus = await _context.PublicationStatuses
                .Include(x => x.Node)
                .FirstOrDefaultAsync(e => e.NodeId == id && e.Status == PublicationStatus.Draft);
            if (draftStatus == null)
                return null;

            var draft = draftStatus.Node;

            draft.Name = updatedNode.Title;
            draft.Props = JsonSerializer.Serialize(updatedNode);

            await _context.History.AddAsync(new History
            {
                NodeId = draft.Id,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = accountId
            });

            await _context.SaveChangesAsync();
            return draft;
        }


        private async Task<Node?> AddTaskAsync(
            Node task,
            Guid accountId,
            Guid? columnId,
            PublicationStatus publicationStatus,
            TaskAttachedMessage? taskAttachedMessage = null)
        {
            if (task == null)
                return null;

            task = (await _context.Nodes.AddAsync(task)).Entity;

            if (columnId != null)
            {
                var nodeLink = new NodeLink()
                {
                    ParentId = columnId.Value,
                    ParentType = NodeType.Column,
                    ChildId = task.Id,
                    ChildType = NodeType.Task,
                    RelationType = RelationType.Contains
                };

                await _context.NodeLinks.AddAsync(nodeLink);
            }



            if (taskAttachedMessage != null)
            {
                _context.NodeLinks.Add(new NodeLink()
                {
                    ParentId = taskAttachedMessage.MessageId,
                    ParentType = NodeType.Message,
                    ChildId = task.Id,
                    ChildType = NodeType.Task,
                    RelationType = RelationType.Attach
                });
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

            _notifyService.Publish(createTaskChatEvent, PublishEvent.CreateTaskChatResponse);

            if (columnId != null)
            {
                var boardMembers = await _context.NodeLinks.Where(e => e.ChildId == columnId)
                    .Join(_context.Nodes,
                        n => n.ParentId,
                        b => b.Id,
                        (n, b) => b)
                    .Join(_context.AccessRights,
                        b => b.Id, 
                        a => a.NodeId,
                        (n, a) => a)
                    .Select(a => a.AccountId)
                    .Where(id => id.HasValue) 
                    .Select(id => id.Value)  
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

        public async Task<IEnumerable<Node>> GetAll(Guid columnId, Guid userId)
        {
            var result = await _context.NodeLinks
                .Where(e => e.ParentId == columnId)
                .Join(_context.Nodes,
                    n1 => n1.ChildId,
                    t => t.Id,
                    (n1, t) => t)
                .Join(_context.History,
                    n => n.Id,
                    h => h.NodeId,
                    (n, h) => h)
                .Include(n => n.Node)
                .Where(x => x.CreatedBy == userId)
                .ToListAsync();

            return result.Select(x => x.Node);
        }



        public async Task AssignTaskToColumn(Guid taskId, Guid columnId)
        {
            var columnTask = await _context.NodeLinks.FirstOrDefaultAsync(e => e.ParentId == columnId && e.ChildId == taskId);

            if (columnTask != null)
                return;

            columnTask = new NodeLink
            {
                ParentId = columnId,
                ParentType = NodeType.Column,
                ChildId = taskId,
                ChildType = NodeType.Task,
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