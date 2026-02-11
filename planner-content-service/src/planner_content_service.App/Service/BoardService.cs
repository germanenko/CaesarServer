using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using System.Net;

namespace planner_content_service.App.Service
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly INotifyService _notifyService;

        public BoardService(IBoardRepository boardRepository, INotifyService notifyService)
        {
            _boardRepository = boardRepository;
            _notifyService = notifyService;
        }

        public async Task<ServiceResponse<ColumnBody>> AddColumn(Guid accountId, ColumnBody column)
        {
            CreateColumnEvent columnEvent = new CreateColumnEvent()
            {
                Column = BodyConverter.ClientToServerBody(column),
                CreatorId = accountId
            };

            var nodeComplete = await _notifyService.Publish(columnEvent, PublishEvent.CreateColumn);

            if (!nodeComplete.IsSuccess)
            {
                return new ServiceResponse<ColumnBody>
                {
                    IsSuccess = nodeComplete.IsSuccess,
                    StatusCode = nodeComplete.StatusCode,
                    Errors = nodeComplete.Errors
                };
            }

            var result = await _boardRepository.AddBoardColumn(column, accountId);

            if (result == null)
            {
                return new ServiceResponse<ColumnBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var body = result.ToColumnBody();

            body.Link = column.Link;

            return new ServiceResponse<ColumnBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = body
            };
        }

        public async Task<ServiceResponse<List<ColumnBody>>> AddColumns(Guid accountId, List<ColumnBody> columns)
        {
            List<ColumnBody>? newColumns = new List<ColumnBody>();
            List<ColumnBody> columnsToStore = new List<ColumnBody>();
            foreach (var column in columns)
            {
                var checkAccess = new CheckAccessRequest()
                {
                    AccountId = accountId,
                    NodeId = column.Id
                };

                var hasAccess = await _notifyService.Publish(checkAccess, PublishEvent.CheckAccess);

                if (hasAccess.IsSuccess)
                {
                    columnsToStore.Add(column);
                }
            }

            newColumns = (await _boardRepository.AddBoardColumns(columnsToStore, accountId)).Select(x => x.ToColumnBody()).ToList();

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
            var boardEvent = new CreateBoardEvent()
            {
                Board = BodyConverter.ClientToServerBody(body),
                CreatorId = accountId
            };

            var nodeComplete = await _notifyService.Publish(boardEvent, PublishEvent.CreateBoard);

            if (!nodeComplete.IsSuccess)
            {
                return new ServiceResponse<BoardBody>
                {
                    IsSuccess = nodeComplete.IsSuccess,
                    StatusCode = nodeComplete.StatusCode,
                    Errors = nodeComplete.Errors
                };
            }

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
                Body = result
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
                Body = result
            };
        }
    }
}