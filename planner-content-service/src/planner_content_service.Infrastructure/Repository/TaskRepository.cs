using Microsoft.AspNetCore.Mvc;
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
            Guid accountId,
            CancellationToken cancellationToken
        ) where T : JobBodyRequest
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

                var result = task.ToTaskBody();

                result = await SetReadStateToJobBody(accountId, result, cancellationToken);

                return result;
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
            string snapshot,
            CancellationToken cancellationToken
        ) where T : JobBodyRequest
        {
            var job = _jobFactory.Create(taskBody);

            job.SetCommon(taskBody.Id, NodeType.Job, taskBody.Name, taskBody.Props);

            try
            {
                var task = (await _context.AddAsync(job)).Entity;

                await _context.ReadStates.AddAsync(new ReadState(task.Id, accountId));

                var primaryAttachedMessage = new AttachedMessage(task.Id, messageId, snapshot);

                await _context.SaveChangesAsync();

                await _context.AttachedMessages.AddAsync(primaryAttachedMessage);

                task = task.WithPrimarySourceMessage(primaryAttachedMessage);

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

                var result = task.ToTaskBody();

                result = await SetReadStateToJobBody(accountId, result, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания задачи: {ex.Message}");

                throw;
            }
        }

        public async Task<JobBody> SetReadStateToJobBody(Guid accountId, JobBody body, CancellationToken cancellationToken)
        {
            var readState = await GetOrCreateReadStateAsync(accountId, body.Id, cancellationToken);

            body.ReadState = readState;

            return body;
        }

        public async Task<AttachedMessage> AttachMessage(Guid accountId, Guid jobId, Guid messageId, string snapshot, CancellationToken cancellationToken)
        {
            var attachedMessage = (await _context.AttachedMessages.AddAsync(new AttachedMessage(jobId, messageId, snapshot))).Entity;

            var readState = await _context.ReadStates.FirstOrDefaultAsync(x => x.JobId == jobId && x.AccountId == accountId);

            if (readState != null)
            {
                readState.LastReadAtUtc = DateTime.UtcNow;
            }
            else
            {
                await _context.ReadStates.AddAsync(new ReadState(jobId, accountId));
            }

            await _context.SaveChangesAsync();

            return attachedMessage;
        }

        public async System.Threading.Tasks.Task SetMessageEdited(Guid messageId, MessageState state, CancellationToken cancellationToken)
        {
            var messages = await _context.AttachedMessages.Where(x => x.MessageId == messageId).ToListAsync();

            foreach (var message in messages)
            {
                message.State = state;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<AttachedMessage?> GetAttachedMessage(Guid jobId, Guid messageId, CancellationToken cancellationToken)
        {
            var attachedMessage = await _context.AttachedMessages.AsNoTracking().FirstOrDefaultAsync(x => x.JobId == jobId && x.MessageId == messageId);

            return attachedMessage;
        }

        public async Task<ReadStateBody> GetOrCreateReadStateAsync(Guid accountId, Guid jobId, CancellationToken cancellationToken)
        {
            var readState = await GetOrCreateReadState(accountId, jobId, cancellationToken);

            if (readState == null)
            {
                readState = (await _context.ReadStates.AddAsync(new ReadState(jobId, accountId))).Entity;
            }

            var result = await FillReadState(readState.ToBody(), cancellationToken);

            return result;
        }

        public async Task<ReadState> GetOrCreateReadState(Guid accountId, Guid jobId, CancellationToken cancellationToken)
        {
            var readState = await _context.ReadStates.FirstOrDefaultAsync(x => x.JobId == jobId && x.AccountId == accountId);

            if (readState == null)
            {
                readState = (await _context.ReadStates.AddAsync(new ReadState(jobId, accountId))).Entity;
            }

            return readState;
        }

        public async Task<ReadStateBody> UpdateReadState(Guid accountId, Guid jobId, CancellationToken cancellationToken)
        {
            var readState = await GetOrCreateReadState(accountId, jobId, cancellationToken);

            readState.LastReadAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = await FillReadState(readState.ToBody(), cancellationToken);

            return result;
        }

        public async Task<ReadStateBody> FillReadState(ReadStateBody body, CancellationToken cancellationToken)
        {
            body.AttachedLastPreview = (await _context.AttachedMessages.OrderByDescending(x => x.AttachedAtUtc).FirstOrDefaultAsync())?.Snapshot;
            body.AttachedUnreadCount = (await _context.AttachedMessages.Where(x => x.AttachedAtUtc > body.LastReadAtUtc).ToListAsync()).Count;

            return body;
        }

        public IEnumerable<JobBody?>? GetAll(List<Guid> ids, CancellationToken cancellationToken)
        {
            var result = _context.Nodes
                .Where(x => ids.Contains(x.Id) && x.Type == NodeType.Job)
                .Select(x => x as Core.Entities.Models.Job)
                .AsEnumerable();

            return result.Select(x => x?.ToTaskBody());
        }


        public async Task<JobBody?> GetAsync(Guid id, CancellationToken cancellationToken)
            => (await _context.Tasks
                .FirstOrDefaultAsync(e => e.Id == id))?.ToTaskBody();

        public async Task<bool> RemoveAsync(
            Guid id,
            CancellationToken cancellationToken)
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
            DateTime changeDate,
            CancellationToken cancellationToken)
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

            var result = task.ToTaskBody();

            result = await SetReadStateToJobBody(accountId, result, cancellationToken);

            return result;
        }
    }
}