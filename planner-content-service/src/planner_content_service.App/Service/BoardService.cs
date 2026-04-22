using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_client_package.Interface;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_server_package;
using planner_server_package.Access;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.Idempotency.Enum;
using planner_server_package.RabbitMQ;
using System.Net;
using System.Text.Json;

namespace planner_content_service.App.Service
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IAccessService _accessService;
        private readonly IPublisherService _publisherService;
        private readonly ILogger<BoardService> _logger;

        public BoardService(IBoardRepository boardRepository, IPublisherService publisherService, ILogger<BoardService> logger, IAccessService accessService)
        {
            _boardRepository = boardRepository;
            _publisherService = publisherService;
            _logger = logger;
            _accessService = accessService;
        }

        public async Task<ServiceResponse<ColumnBody>> CreateOrUpdateColumn(Guid accountId, ColumnBody column)
        {
            CreateNodeEvent columnEvent = new CreateNodeEvent()
            {
                Node = BodyConverter.ClientToServerBody(column),
                CreatorId = accountId
            };

            var response = await _publisherService.Publish(columnEvent, PublishEvent.CreateNode);

            if (!response.IsSuccess)
            {
                return new ServiceResponse<ColumnBody>
                {
                    IsSuccess = response.IsSuccess,
                    StatusCode = response.StatusCode,
                    Errors = response.Errors,
                    ErrorCodes = response.ErrorCodes
                };
            }

            NodeBody responseBody = new NodeBody();
            if (response.Body != null)
            {
                if (response.Body is JsonElement jsonElement)
                {
                    responseBody = JsonSerializer.Deserialize<NodeBody>(jsonElement);
                }
            }

            var result = await _boardRepository.CreateOrUpdateColumn(column, accountId, responseBody);

            if (result == null)
            {
                return new ServiceResponse<ColumnBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorCodes = [ErrorCode.Infrastructure]
                };
            }

            return new ServiceResponse<ColumnBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<bool>> DeleteNode(Guid accountId, Guid columnId)
        {
            DeleteNodeEvent deleteEvent = new DeleteNodeEvent()
            {
                NodeId = columnId,
                AccountId = accountId
            };

            var request = await _publisherService.Publish(deleteEvent, PublishEvent.DeleteNode);

            if (!request.IsSuccess)
            {
                return new ServiceResponse<bool>
                {
                    IsSuccess = request.IsSuccess,
                    StatusCode = request.StatusCode,
                    Errors = request.Errors
                };
            }

            var result = await _boardRepository.DeleteNode(columnId, accountId);

            if (!result)
            {
                return new ServiceResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Нода не удалена" },
                    ErrorCodes = [ErrorCode.Infrastructure]
                };
            }

            return new ServiceResponse<bool>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<List<ColumnBody>>> CreateBaseColumns(Guid accountId, Guid boardId)
        {
            var columnId = Guid.NewGuid();

            ColumnBody reminderColumn = new ColumnBody()
            {
                Id = columnId,
                Name = "Reminders",
                Type = NodeType.Column,
                PublicationStatus = PublicationStatus.Active,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = accountId,
                Link = new NodeLinkBody()
                {
                    Id = Guid.NewGuid(),
                    ChildId = columnId,
                    ParentId = boardId,
                    RelationType = RelationType.Contains
                }
            };

            List<ColumnBody> columns = new List<ColumnBody>();

            columns.Add(reminderColumn);

            foreach (var column in columns)
            {
                CreateNodeEvent columnEvent = new CreateNodeEvent()
                {
                    Node = BodyConverter.ClientToServerBody(reminderColumn),
                    CreatorId = accountId
                };

                var request = await _publisherService.Publish(columnEvent, PublishEvent.CreateNode);

                if (!request.IsSuccess)
                {
                    return new ServiceResponse<List<ColumnBody>>
                    {
                        IsSuccess = request.IsSuccess,
                        StatusCode = request.StatusCode,
                        Errors = request.Errors
                    };
                }
            }


            var result = await _boardRepository.CreateOtUpdateColumns(columns, accountId);

            if (result == null)
            {
                return new ServiceResponse<List<ColumnBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorCodes = [ErrorCode.Infrastructure]
                };
            }

            return new ServiceResponse<List<ColumnBody>>
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
                var hasAccess = await _accessService.CheckAccess(accountId, column.Id, Permission.Write);

                if (hasAccess)
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
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorCodes = [ErrorCode.Infrastructure]
                };
            }

            return new ServiceResponse<List<ColumnBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = newColumns
            };
        }

        public async Task<ServiceResponse<BoardBody>> CreateOrUpdateBoardAsync(CreateOrUpdateBoardBody body, Guid accountId)
        {
            var boardBody = new BoardBody() { Id = body.Id, Name = body.Name, Props = body.Props, Type = NodeType.Board, UpdatedBy = accountId, SyncKind = SyncKind.Scope };

            var boardEvent = new CreateNodeEvent()
            {
                Node = BodyConverter.ClientToServerBody(boardBody),
                CreatorId = accountId
            };

            var response = await _publisherService.Publish(boardEvent, PublishEvent.CreateNode);

            if (!response.IsSuccess)
            {
                return new ServiceResponse<BoardBody>
                {
                    IsSuccess = response.IsSuccess,
                    StatusCode = response.StatusCode,
                    Errors = response.Errors
                };
            }

            NodeBody responseBody = new NodeBody();
            if (response.Body != null)
            {
                if (response.Body is JsonElement jsonElement)
                {
                    responseBody = JsonSerializer.Deserialize<NodeBody>(jsonElement);
                }
            }

            var hasBoard = await _boardRepository.GetBoardById(body.Id);

            var result = await _boardRepository.CreateOrUpdateBoardAsync(boardBody, accountId, responseBody);

            if (result is null)
            {
                return new ServiceResponse<BoardBody>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorCodes = [ErrorCode.Infrastructure]
                };
            }

            if (hasBoard == null)
            {
                var columns = await CreateBaseColumns(accountId, body.Id);

                result.Childs = columns.Body;
            }

            return new ServiceResponse<BoardBody>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<List<BoardBody>>> CreateOrUpdateBoards(List<CreateOrUpdateBoardBody> bodies, Guid accountId)
        {
            var boardBodies = bodies.Select(body => new BoardBody() { Id = body.Id, Name = body.Name, Props = body.Props, Type = NodeType.Board, UpdatedBy = accountId }).ToList();

            var result = await _boardRepository.CreateOrUpdateBoards(boardBodies, accountId);

            if (result is null)
            {
                return new ServiceResponse<List<BoardBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorCodes = [ErrorCode.Infrastructure]
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

        public async Task<ServiceResponse<Guid>> AddDefaultColumn(Guid accountId, Guid columnId)
        {
            return await AddDefaultColumnInternal(accountId, columnId);
        }

        public async Task<ServiceResponse<Guid>> AddDefaultColumnForChat(Guid accountId, Guid columnId, Guid chatId)
        {
            return await AddDefaultColumnInternal(accountId, columnId, chatId);
        }

        private async Task<ServiceResponse<Guid>> AddDefaultColumnInternal(Guid accountId, Guid columnId, Guid? chatId = null)
        {
            var existingColumn = await _boardRepository.GetColumnById(columnId);

            if (existingColumn == null)
                return new ServiceResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var existingTaskColumn = await _boardRepository.GetUserTaskColumn(accountId, columnId, null);

            if (existingTaskColumn != null)
                return new ServiceResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var taskColumnId = await _boardRepository.AddTaskColumn(accountId, columnId);

            return new ServiceResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = taskColumnId
            };
        }

        public async Task<ServiceResponse<List<ColumnBody>>> GetDefaultColumns(Guid accountId, Guid? chatId)
        {
            var columns = await _boardRepository.GetUserTaskColumns(accountId, chatId);

            if (columns.IsNullOrEmpty())
            {
                return new ServiceResponse<List<ColumnBody>>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            return new ServiceResponse<List<ColumnBody>>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = columns
            };
        }
    }
}