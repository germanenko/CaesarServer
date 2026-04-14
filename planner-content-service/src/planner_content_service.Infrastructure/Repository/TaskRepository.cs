using Microsoft.EntityFrameworkCore;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Infrastructure.Data;
using planner_server_package.Events;
using planner_server_package.RabbitMQ;

namespace planner_content_service.Infrastructure.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ContentDbContext _context;
        private readonly IPublisherService _publisherService;

        public TaskRepository(ContentDbContext context, IPublisherService publisherService)
        {
            _context = context;
            _publisherService = publisherService;
        }

        public async Task<TaskBody?> AddAsync
        (
            TaskBody taskBody,
            Guid accountId
        )
        {
            var taskModel = new Core.Entities.Models.Job()
            {
                Id = taskBody.Id,
                Name = taskBody.Name,
                Type = NodeType.Task,
                Description = taskBody.Description,
                EndDate = taskBody.EndDate,
                HexColor = taskBody.HexColor,
                Props = taskBody.Props,
                StartDate = taskBody.StartDate
            };

            try
            {

                var task = (await _context.Jobs.AddAsync(taskModel)).Entity;

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

                return taskBody;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания задачи: {ex.Message}");

                throw;
            }
        }

        public IEnumerable<TaskBody?>? GetAll(List<Guid> ids)
        {
            var result = _context.Nodes
                .Where(x => ids.Contains(x.Id) && x.Type == NodeType.Task)
                .Select(x => x as Core.Entities.Models.Job)
                .AsEnumerable();

            return result.Select(x => x?.ToTaskBody());
        }


        public async Task<TaskBody?> GetAsync(Guid id)
            => (await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id))?.ToTaskBody();

        public async Task<bool> RemoveAsync(Guid id)
        {
            var task = await _context.Nodes.FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
                return true;

            _context.Nodes.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskBody?> UpdateAsync(
            Guid id,
            Guid accountId,
            TaskBody updatedNode,
            DateTime changeDate)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id);

            if (task == null)
                return null;

            task.StartDate = updatedNode.StartDate;
            task.EndDate = updatedNode.EndDate;
            task.Description = updatedNode.Description;
            task.HexColor = updatedNode.HexColor;
            task.Name = updatedNode.Name;
            task.Props = updatedNode.Props;

            await _context.SaveChangesAsync();
            return updatedNode;
        }
    }
}