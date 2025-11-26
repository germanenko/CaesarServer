using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
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

        public async Task<HttpStatusCode> AddBoardMemberAsync(Guid boardId, Guid accountId, Guid newAccountId, AccessType accessType)
        {
            var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, boardId);
            if (boardMember is null)
                return HttpStatusCode.Forbidden;

            var result = await _boardRepository.AddBoardMember(newAccountId, boardId, accessType);
            return result == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK;
        }

        public async Task<ServiceResponse<NodeBody>> AddColumn(Guid accountId, CreateColumnBody column)
        {
            var result = await _boardRepository.AddBoardColumn(column, accountId);

            if(result == null)
            {
                return new ServiceResponse<NodeBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new ServiceResponse<NodeBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.ToNodeBody()
            };
        }

        public async Task<ServiceResponse<List<NodeBody>>> AddColumns(Guid accountId, List<CreateColumnBody> columns)
        {
            List<NodeBody>? newColumns = new List<NodeBody>();
            foreach (var column in columns)
            {
                var board = await _boardRepository.GetBoard(column.Id);
                var boardMember = await _boardRepository.GetBoardMemberAsync(accountId, board.Id);
                if (boardMember == null)
                {
                    return new ServiceResponse<List<NodeBody>>
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.Forbidden
                    };
                }
            }

            newColumns = (await _boardRepository.AddBoardColumns(columns, accountId)).Select(x => x.ToNodeBody()).ToList();

            if (newColumns == null)
            {
                return new ServiceResponse<List<NodeBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new ServiceResponse<List<NodeBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = newColumns
            };
        }

        public async Task<ServiceResponse<NodeBody>> CreateBoardAsync(CreateBoardBody body, Guid accountId)
        {
            var result = await _boardRepository.AddAsync(body, accountId);

            if (result is null)
            {
                return new ServiceResponse<NodeBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }

            return new ServiceResponse<NodeBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.ToNodeBody()
            };
        }

        public async Task<ServiceResponse<List<NodeBody>>> CreateBoardsAsync(List<CreateBoardBody> bodies, Guid accountId)
        {
            var result = await _boardRepository.AddRangeAsync(bodies, accountId);

            if (result is null)
            {
                return new ServiceResponse<List<NodeBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }

            return new ServiceResponse<List<NodeBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.Select(x => x.ToNodeBody()).ToList()
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