using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using System.Net;
using System.Xml.Linq;

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

        public async Task<ServiceResponse<BoardColumnBody>> AddColumn(Guid accountId, CreateColumnBody column)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, column.BoardId);
            if (boardMember == null)
            {
                return new ServiceResponse<BoardColumnBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Forbidden
                };
            }

            var board = await _boardRepository.GetAsync(column.BoardId);
            if (board == null)
            {
                return new ServiceResponse<BoardColumnBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var result = await _boardRepository.AddBoardColumn(board, column, accountId);

            if(result == null)
            {
                return new ServiceResponse<BoardColumnBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new ServiceResponse<BoardColumnBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.ToBoardColumnBody()
            };
        }

        public async Task<ServiceResponse<List<BoardColumnBody>>> AddColumns(Guid accountId, List<CreateColumnBody> columns)
        {
            List<BoardColumn>? newColumns = new List<BoardColumn>();
            foreach (var column in columns)
            {
                var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, column.BoardId);
                if (boardMember == null)
                {
                    return new ServiceResponse<List<BoardColumnBody>>
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.Forbidden
                    };
                }

                var board = await _boardRepository.GetAsync(column.BoardId);
                if (board == null)
                {
                    return new ServiceResponse<List<BoardColumnBody>>
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest
                    };
                } 
            }

            newColumns = await _boardRepository.AddBoardColumns(columns, accountId);

            if (newColumns == null)
            {
                return new ServiceResponse<List<BoardColumnBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new ServiceResponse<List<BoardColumnBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = newColumns.Select(x => x.ToBoardColumnBody()).ToList()
            };
        }

        public async Task<ServiceResponse<BoardBody>> CreateBoardAsync(CreateBoardBody body, Guid accountId)
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

        public async Task<ServiceResponse<List<BoardBody>>> CreateBoardsAsync(List<CreateBoardBody> bodies, Guid accountId)
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

        public async Task<ServiceResponse<IEnumerable<BoardColumnBody>>> GetAllBoardColumnsAsync(Guid accountId)
        {
            var result = await _boardRepository.GetAllBoardColumns(accountId);

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