using System.Net;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;

namespace Planer_task_board.App.Service
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;

        public BoardService(IBoardRepository boardRepository)
        {
            _boardRepository = boardRepository;
        }

        public async Task<HttpStatusCode> AddBoardMemberAsync(Guid boardId, Guid accountId, Guid newAccountId)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember is null)
                return HttpStatusCode.Forbidden;

            var result = await _boardRepository.AddBoardMember(newAccountId, boardId);
            return result == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> AddColumn(Guid accountId, Guid boardId, string name)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember == null)
                return HttpStatusCode.Forbidden;

            var board = await _boardRepository.GetAsync(boardId);
            if (board == null)
                return HttpStatusCode.BadRequest;

            var result = await _boardRepository.AddBoardColumn(board, name);
            return result == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK;
        }

        public async Task<ServiceResponse<BoardBody>> CreateBoardAsync(CreateBoardBody body, Guid accountId)
        {
            var result = await _boardRepository.AddAsync(body.Name, accountId);

            if (result is null)
            {
                return new ServiceResponse<BoardBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }

            return new ServiceResponse<BoardBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.ToBoardBody()
            };
        }

        public async Task<ServiceResponse<IEnumerable<BoardColumnBody>>> GetBoardColumnsAsync(Guid boardId)
        {
            var result = await _boardRepository.GetBoardColumns(boardId);

            return new ServiceResponse<IEnumerable<BoardColumnBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.Select(b => b.ToBoardColumnBody())
            };
        }

        public async Task<ServiceResponse<IEnumerable<Guid>>> GetBoardMembersAsync(Guid boardId, int count, int offset)
        {
            var result = await _boardRepository.GetBoardMembers(boardId, count, offset);

            return new ServiceResponse<IEnumerable<Guid>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<IEnumerable<BoardBody>>> GetBoardsAsync(Guid accountId)
        {
            var result = await _boardRepository.GetAll(accountId);

            return new ServiceResponse<IEnumerable<BoardBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.Select(b => b.ToBoardBody())
            };
        }
    }
}