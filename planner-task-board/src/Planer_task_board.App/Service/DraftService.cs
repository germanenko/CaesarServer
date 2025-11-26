using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Planer_task_board.App.Service
{
    public class DraftService : IDraftService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly ITaskRepository _taskRepository;

        public DraftService(
            IBoardRepository boardRepository,
            ITaskRepository taskRepository)
        {
            _boardRepository = boardRepository;
            _taskRepository = taskRepository;
        }

        public async Task<ServiceResponse<NodeBody>> ConvertDraftToTask(Guid accountId, Guid boardId, Guid draftId, Guid columnId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<NodeBody>
                {
                    Errors = new[] { "You are not a member of this board" },
                    StatusCode = HttpStatusCode.Forbidden,
                    IsSuccess = false
                };

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return new ServiceResponse<NodeBody>
                {
                    Errors = new[] { "Column not found" },
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false
                };

            var result = await _taskRepository.ConvertDraftToTask(draftId, accountId, column.Id);
            if (result == null)
                return new ServiceResponse<NodeBody>
                {
                    Errors = new[] { "Failed to convert draft to task" },
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            return new ServiceResponse<NodeBody>
            {
                Body = result.ToNodeBody(),
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<NodeBody>> CreateDraft(CreateOrUpdateTaskBody body, Guid accountId, Guid columnId)
        {
            var errors = new List<string>();

            if (body.StartDate != null && !DateTime.TryParse(body?.StartDate, out var _))
                errors.Add("Start time format is not correct");

            if (body.EndDate != null && !DateTime.TryParse(body.EndDate, out var _))
                errors.Add("End time format is not correct");

            if (errors.Any())
                return new ServiceResponse<NodeBody>
                {
                    Errors = errors.ToArray(),
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return new ServiceResponse<NodeBody>
                {
                    Errors = new[] { "Column not found" },
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false
                };

            //!вернуть при разработке логики доступов
            //var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, column.BoardId);
            //if (boardMember == null)
            //    return new ServiceResponse<Node>
            //    {
            //        Errors = new[] { "You are not a member of this board" },
            //        StatusCode = HttpStatusCode.Forbidden,
            //        IsSuccess = false
            //    };

            Node task = null;

            task = await _taskRepository.GetAsync(body.Id, false);

            if (task == null)
                return new ServiceResponse<NodeBody>
                {
                    Errors = new[] { "taskId is not found" },
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };


            var result = await _taskRepository.AddAsync(JsonSerializer.Deserialize<CreateOrUpdateTaskBody>(task.Props), accountId);

            if (result == null)
                return new ServiceResponse<NodeBody>
                {
                    Errors = new[] { "Failed to create draft" },
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            return new ServiceResponse<NodeBody>
            {
                Body = result.ToNodeBody(),
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<IEnumerable<NodeBody>>> GetDrafts(Guid accountId, Guid boardId, Guid columnId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<IEnumerable<NodeBody>>
                {
                    Errors = new[] { "You are not a member of this board" },
                    StatusCode = HttpStatusCode.Forbidden,
                    IsSuccess = false
                };

            var tasks = await _taskRepository.GetAll(columnId, true);
            return new ServiceResponse<IEnumerable<NodeBody>>
            {
                Body = tasks.Select(x => x.ToNodeBody()),
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<NodeBody>> UpdateDraft(Guid accountId, Guid draftId, CreateOrUpdateTaskBody body)
        {
            var errors = new List<string>();

            if (body.StartDate != null && !DateTime.TryParse(body?.StartDate, out var _))
                errors.Add("Start time format is not correct");

            if (body.EndDate != null && !DateTime.TryParse(body.EndDate, out var _))
                errors.Add("End time format is not correct");

            if (errors.Any())
                return new ServiceResponse<NodeBody>
                {
                    Errors = errors.ToArray(),
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            Node? draftOfTask = null;

            draftOfTask = await _taskRepository.GetAsync(body.Id, false);

            if (draftOfTask == null)
                return new ServiceResponse<NodeBody>
                {
                    Errors = new[] { "taskId is not found" },
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            DateTime? startDate = body.StartDate != null ? DateTime.Parse(body.StartDate) : null;
            DateTime? endDate = body.EndDate != null ? DateTime.Parse(body.EndDate) : null;
            var result = await _taskRepository.UpdateAsync(body.Id, JsonSerializer.Deserialize<CreateOrUpdateTaskBody>(draftOfTask.Props), body.UpdatedAt);
            return result == null ? new ServiceResponse<NodeBody>
            {
                Errors = new[] { "Failed to update draft" },
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false
            } : new ServiceResponse<NodeBody>
            {
                Body = result.ToNodeBody(),
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }
    }
}