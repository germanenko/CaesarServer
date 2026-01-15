using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using System.Net;

namespace planner_content_service.App.Service
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;

        public BoardService(IBoardRepository boardRepository)
        {
            _boardRepository = boardRepository;
        }

        public async Task<HttpStatusCode> AddBoardMemberAsync(Guid boardId, Guid accountId, Guid newAccountId, AccessType accessType)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember is null)
                return HttpStatusCode.Forbidden;

            var result = await _boardRepository.AddBoardMember(newAccountId, boardId, accessType);
            return result == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK;
        }

        public async Task<ServiceResponse<ColumnBody>> AddColumn(Guid accountId, ColumnBody column)
        {
            var result = await _boardRepository.AddBoardColumn(column, accountId);

            if(result == null)
            {
                return new ServiceResponse<ColumnBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new ServiceResponse<ColumnBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.ToColumnBody()
            };
        }

        public async Task<ServiceResponse<List<ColumnBody>>> AddColumns(Guid accountId, List<ColumnBody> columns)
        {
            List<ColumnBody>? newColumns = new List<ColumnBody>();
            foreach (var column in columns)
            {
                var board = await _boardRepository.GetBoard(column.Id);
                var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, board.Id);
                if (boardMember == null)
                {
                    return new ServiceResponse<List<ColumnBody>>
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.Forbidden
                    };
                }
            }

            newColumns = (await _boardRepository.AddBoardColumns(columns, accountId)).Select(x => x.ToColumnBody()).ToList();

            if (newColumns == null)
            {
                return new ServiceResponse<List<ColumnBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new ServiceResponse<List<ColumnBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = newColumns
            };
        }

        public async Task<ServiceResponse<BoardBody>> CreateBoardAsync(BoardBody body, Guid accountId)
        {
            var result = await _boardRepository.AddAsync(body, accountId);

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

        public async Task<ServiceResponse<List<BoardBody>>> CreateBoardsAsync(List<BoardBody> bodies, Guid accountId)
        {
            var result = await _boardRepository.AddRangeAsync(bodies, accountId);

            if (result is null)
            {
                return new ServiceResponse<List<BoardBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }

            return new ServiceResponse<List<BoardBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.Select(x => x.ToBoardBody()).ToList()
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
    }
}