using planner_client_package.Entities;
using planner_client_package.Interface;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Net;

namespace planner_content_service.App.Service
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IPublisherService _publisherService;

        public BoardService(IBoardRepository boardRepository, IPublisherService publisherService)
        {
            _boardRepository = boardRepository;
            _publisherService = publisherService;
        }

        public async Task<ServiceResponse<ColumnBody>> CreateOrUpdateColumn(Guid accountId, ColumnBody column)
        {
            CreateColumnEvent columnEvent = new CreateColumnEvent()
            {
                Column = BodyConverter.ClientToServerBody(column),
                CreatorId = accountId
            };

            var request = await _publisherService.Publish(columnEvent, PublishEvent.CreateColumn);

            if (!request.IsSuccess)
            {
                return new ServiceResponse<ColumnBody>
                {
                    IsSuccess = request.IsSuccess,
                    StatusCode = request.StatusCode,
                    Errors = request.Errors
                };
            }

            var result = await _boardRepository.CreateOrUpdateColumn(column, accountId);

            if (result == null)
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
                Body = result
            };
        }

        public async Task<ServiceResponse<ColumnBody>> CreateBaseColumns(Guid accountId, Guid boardId)
        {
            var columnId = Guid.NewGuid();

            ColumnBody reminderColumn = new ColumnBody()
            {
                Id = columnId,
                Name = "Reminders",
                PublicationStatus = PublicationStatus.Active,
                UpdatedAt = DateTime.UtcNow,
                Link = new NodeLinkBody()
                {
                    Id = Guid.NewGuid(),
                    ChildId = columnId,
                    ParentId = boardId,
                    RelationType = RelationType.Contains
                },
                AccessRight = new AccessRightBody()
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    NodeId = columnId,
                    Permission = Permission.Creator
                }
            };

            CreateColumnEvent columnEvent = new CreateColumnEvent()
            {
                Column = BodyConverter.ClientToServerBody(reminderColumn),
                CreatorId = accountId
            };

            var request = await _publisherService.Publish(columnEvent, PublishEvent.CreateColumn);

            if (!request.IsSuccess)
            {
                return new ServiceResponse<ColumnBody>
                {
                    IsSuccess = request.IsSuccess,
                    StatusCode = request.StatusCode,
                    Errors = request.Errors
                };
            }

            var result = await _boardRepository.CreateOrUpdateColumn(reminderColumn, accountId);

            if (result == null)
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
                Body = result
            };
        }

        public async Task<ServiceResponse<List<ColumnBody>>> CreateOrUpdateColumns(Guid accountId, List<ColumnBody> columns)
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

                var hasAccess = await _publisherService.Publish(checkAccess, PublishEvent.CheckAccess);

                if (hasAccess.IsSuccess)
                {
                    columnsToStore.Add(column);
                }
            }

            newColumns = await _boardRepository.CreateOtUpdateColumns(columnsToStore, accountId);

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

        public async Task<ServiceResponse<BoardBody>> CreateOrUpdateBoardAsync(BoardBody body, Guid accountId)
        {
            var boardEvent = new CreateBoardEvent()
            {
                Board = BodyConverter.ClientToServerBody(body),
                CreatorId = accountId
            };

            var nodeComplete = await _publisherService.Publish(boardEvent, PublishEvent.CreateBoard);

            if (!nodeComplete.IsSuccess)
            {
                return new ServiceResponse<BoardBody>
                {
                    IsSuccess = nodeComplete.IsSuccess,
                    StatusCode = nodeComplete.StatusCode,
                    Errors = nodeComplete.Errors
                };
            }

            var result = await _boardRepository.CreateOrUpdateBoardAsync(body, accountId);

            if (result is null)
            {
                return new ServiceResponse<BoardBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }

            await CreateBaseColumns(accountId, body.Id);

            return new ServiceResponse<BoardBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<List<BoardBody>>> CreateOrUpdateBoards(List<BoardBody> bodies, Guid accountId)
        {
            var result = await _boardRepository.CreateOrUpdateBoards(bodies, accountId);

            if (result is null)
            {
                return new ServiceResponse<List<BoardBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }

            foreach (var board in result)
            {
                await CreateBaseColumns(accountId, board.Id);
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