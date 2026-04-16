using Microsoft.EntityFrameworkCore;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IFactory;
using planner_content_service.Core.IRepository;
using planner_content_service.Infrastructure.Data;
using planner_server_package.Events;
using planner_server_package.RabbitMQ;
using JobBody = planner_client_package.Entities.JobBody;
using JobRequestBody = planner_client_package.Entities.Request.JobBody;

namespace planner_content_service.Infrastructure.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ContentDbContext _context;
        private readonly IPublisherService _publisherService;
        private readonly IJobFactory _jobFactory;

        public TaskRepository(ContentDbContext context, IPublisherService publisherService, IJobFactory jobFactory)
        {
            _context = context;
            _publisherService = publisherService;
            _jobFactory = jobFactory;
        }

        public async Task<JobBody?> AddAsync<T>
        (
            T taskBody,
            Guid accountId
        ) where T : JobRequestBody
        {
            var job = _jobFactory.Create(taskBody);

            job.SetCommon(taskBody.Id, NodeType.Job, taskBody.Name, taskBody.Props);

            try
            {

                var task = (await _context.AddAsync(job)).Entity;

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

                return task.ToTaskBody();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания задачи: {ex.Message}");

                throw;
            }
        }

        public async Task<JobBody?> AddJobFromMessageAsync<T>
        (
            T taskBody,
            Guid accountId,
            Guid messageId,
            string snapshot
        ) where T : JobRequestBody
        {
            var job = _jobFactory.Create(taskBody);

            job.SetCommon(taskBody.Id, NodeType.Job, taskBody.Name, taskBody.Props);

            try
            {
                job = job.WithPrimarySourceMessage(messageId, snapshot);

                var task = (await _context.AddAsync(job)).Entity;

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

                return task.ToTaskBody();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания задачи: {ex.Message}");

                throw;
            }
        }

        public IEnumerable<JobBody?>? GetAll(List<Guid> ids)
        {
            var result = _context.Nodes
                .Where(x => ids.Contains(x.Id) && x.Type == NodeType.Job)
                .Select(x => x as Core.Entities.Models.Job)
                .AsEnumerable();

            return result.Select(x => x?.ToTaskBody());
        }


        public async Task<JobBody?> GetAsync(Guid id)
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

        public async Task<JobBody?> UpdateAsync(
            Guid id,
            Guid accountId,
            JobBody updatedNode,
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