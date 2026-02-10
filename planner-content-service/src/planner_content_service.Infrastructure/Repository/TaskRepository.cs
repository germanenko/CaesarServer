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
                Type = NodeType.Task,
                Description = task.Description,
                EndDate = task.EndDate,
                HexColor = task.HexColor,
                Props = task.Props,
                StartDate = task.StartDate
            };

            _context.SaveChanges();


            return await AddTaskAsync(node, accountId, task.PublicationStatus, task.Link);
        }

        public IEnumerable<TaskModel?>? GetAll(List<Guid> ids)
        {
            var result = _context.Nodes
                .Where(x => ids.Contains(x.Id) && x.Type == NodeType.Task)
                .Select(x => x as TaskModel)
                .AsEnumerable();

            return result;
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


        public async Task<TaskModel?> UpdateAsync(
            Guid id,
            Guid accountId,
            TaskBody updatedNode,
            Guid? columnId,
            DateTime changeDate)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id);
            if (task == null)
                return null;

            task.Name = updatedNode.Name;
            task.Props = JsonSerializer.Serialize(updatedNode);

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
            var task = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id);
            if (task == null)
                return null;

            task.Name = updatedNode.Name;
            task.Props = JsonSerializer.Serialize(updatedNode);

            await _context.SaveChangesAsync();
            return task;
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
    }
}