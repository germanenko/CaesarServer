using System.Net;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;

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

        public async Task<ServiceResponse<Node>> ConvertDraftToTask(Guid accountId, Guid boardId, Guid draftId, Guid columnId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<Node>
                {
                    Errors = new[] { "You are not a member of this board" },
                    StatusCode = HttpStatusCode.Forbidden,
                    IsSuccess = false
                };

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return new ServiceResponse<Node>
                {
                    Errors = new[] { "Column not found" },
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false
                };

            var result = await _taskRepository.ConvertDraftToTask(draftId, accountId, column.Id);
            if (result == null)
                return new ServiceResponse<Node>
                {
                    Errors = new[] { "Failed to convert draft to task" },
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            return new ServiceResponse<Node>
            {
                Body = result,
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<Node>> CreateDraft(Node body, Guid accountId, Guid columnId)
        {
            var errors = new List<string>();

            //if (body.StartDate != null && !DateTime.TryParse(body?.StartDate, out var _))
            //    errors.Add("Start time format is not correct");

            //if (body.EndDate != null && !DateTime.TryParse(body.EndDate, out var _))
            //    errors.Add("End time format is not correct");

            if (errors.Any())
                return new ServiceResponse<Node>
                {
                    Errors = errors.ToArray(),
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            var column = await _boardRepository.GetBoardColumn(columnId);
            if (column == null)
                return new ServiceResponse<Node>
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

            //if (body.ModifiedTaskId != null)
            //{
            //    draftOfTask = await _taskRepository.GetAsync((Guid)body.ModifiedTaskId, false);
            //    if (draftOfTask == null)
            //        return new ServiceResponse<Node>
            //        {
            //            Errors = new[] { "taskId is not found" },
            //            StatusCode = HttpStatusCode.BadRequest,
            //            IsSuccess = false
            //        };
            //}


            var result = await _taskRepository.AddAsync(body);

            if (result == null)
                return new ServiceResponse<Node>
                {
                    Errors = new[] { "Failed to create draft" },
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            return new ServiceResponse<Node>
            {
                Body = result,
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<IEnumerable<Node>>> GetDrafts(Guid accountId, Guid boardId, Guid columnId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<IEnumerable<Node>>
                {
                    Errors = new[] { "You are not a member of this board" },
                    StatusCode = HttpStatusCode.Forbidden,
                    IsSuccess = false
                };

            var tasks = await _taskRepository.GetAll(columnId, true);
            return new ServiceResponse<IEnumerable<Node>>
            {
                Body = tasks,
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<Node>> UpdateDraft(Guid accountId, Guid boardId, Guid draftId, Node body)
        {
            var errors = new List<string>();

            //if (body.StartDate != null && !DateTime.TryParse(body?.StartDate, out var _))
            //    errors.Add("Start time format is not correct");

            //if (body.EndDate != null && !DateTime.TryParse(body.EndDate, out var _))
            //    errors.Add("End time format is not correct");

            if (errors.Any())
                return new ServiceResponse<Node>
                {
                    Errors = errors.ToArray(),
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false
                };

            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return new ServiceResponse<Node>
                {
                    Errors = new[] { "You are not a member of this board" },
                    StatusCode = HttpStatusCode.Forbidden,
                    IsSuccess = false
                };

            Node? draftOfTask = null;
            //if (body.ModifiedTaskId != null)
            //{
            //    draftOfTask = await _taskRepository.GetAsync((Guid)body.ModifiedTaskId, false);
            //    if (draftOfTask == null)
            //        return new ServiceResponse<Node>
            //        {
            //            Errors = new[] { "taskId is not found" },
            //            StatusCode = HttpStatusCode.BadRequest,
            //            IsSuccess = false
            //        };
            //}

            //DateTime? startDate = body.StartDate != null ? DateTime.Parse(body.StartDate) : null;
            //DateTime? endDate = body.EndDate != null ? DateTime.Parse(body.EndDate) : null;
            var result = await _taskRepository.UpdateAsync(body.Id, body, body.UpdatedAt);
            return result == null ? new ServiceResponse<Node>
            {
                Errors = new[] { "Failed to update draft" },
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false
            } : new ServiceResponse<Node>
            {
                Body = result,
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }
    }
}