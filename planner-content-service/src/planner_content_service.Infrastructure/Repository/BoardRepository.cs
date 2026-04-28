using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_content_service.Infrastructure.Data;
using planner_server_package;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace planner_content_service.Infrastructure.Repository
{
    public class BoardRepository : IBoardRepository
    {
        private readonly ContentDbContext _context;
        private readonly IPublisherService _publisherService;
        private readonly ILogger<BoardRepository> _logger;

        public BoardRepository(ContentDbContext context, IPublisherService publisherService, ILogger<BoardRepository> logger)
        {
            _context = context;
            _publisherService = publisherService;
            _logger = logger;
        }

        public async Task<BoardBody?> GetBoardById(Guid boardId, CancellationToken cancellationToken = default)
        {
            return (await _context.Boards.AsNoTracking().FirstOrDefaultAsync(x => x.Id == boardId, cancellationToken))?.ToBoardBody();
        }

        public async Task<ColumnBody?> GetColumnById(Guid columnId, CancellationToken cancellationToken = default)
        {
            return (await _context.Columns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == columnId, cancellationToken))?.ToColumnBody();
        }

        public async Task<Guid?> GetUserTaskColumn(Guid accountId, Guid columnId, Guid? chatId, CancellationToken cancellationToken = default)
        {
            return (await _context.UserTaskColumns.AsNoTracking().FirstOrDefaultAsync(x => x.ColumnId == columnId && x.AccountId == accountId && x.ChatId == chatId, cancellationToken))?.Id;
        }

        public async Task<List<ColumnBody>> GetUserTaskColumns(Guid accountId, Guid? chatId, CancellationToken cancellationToken = default)
        {
            var taskColumns = await _context.UserTaskColumns.Include(x => x.Column).AsNoTracking().Where(x => x.AccountId == accountId && x.ChatId == chatId).Select(x => x.Column).ToListAsync(cancellationToken);

            return taskColumns.Select(x => x.ToColumnBody()).ToList();
        }

        public async Task<Guid> AddTaskColumn(Guid accountId, Guid columnId, CancellationToken cancellationToken = default, Guid? chatId = null)
        {
            var result = (await _context.UserTaskColumns.AddAsync(new UserTaskColumn(accountId, columnId, chatId), cancellationToken)).Entity.ColumnId;

            await _context.SaveChangesAsync(cancellationToken);

            return result;
        }



        public async Task<BoardBody?> CreateOrUpdateBoardAsync(
            BoardBody boardBody,
            Guid accountId,
            NodeBody metadata,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Props: {boardBody.Props}");
                var board = await _context.Boards.FirstOrDefaultAsync(x => x.Id == boardBody.Id, cancellationToken);

                if (board != null)
                {
                    board.Name = boardBody.Name;
                    board.Props = boardBody.Props;

                    await _context.SaveChangesAsync(cancellationToken);

                    return board.ToBoardBody();
                }

                var newBoard = new Board
                {
                    Id = boardBody.Id != Guid.Empty ? boardBody.Id : Guid.NewGuid(),
                    Name = boardBody.Name,
                    Type = NodeType.Board,
                    Props = boardBody.Props
                };

                await _context.Boards.AddAsync(newBoard, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                return newBoard.ToBoardBody().ApplyNodeMetadata(metadata);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании доски {boardBody.Name}: {ex.Message}");

                throw;
            }
        }

        public async Task<List<BoardBody>?> CreateOrUpdateBoards(
            List<BoardBody> boards,
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            List<BoardBody> newBoardNodes = new List<BoardBody>();

            foreach (var board in boards)
            {
                newBoardNodes.Add(await CreateOrUpdateBoardAsync(board, accountId, board, cancellationToken));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return newBoardNodes;
        }


        public async Task<ColumnBody?> CreateOrUpdateColumn(
            ColumnBody columnBody,
            Guid accountId,
            NodeBody metadata,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var column = await _context.Columns.FirstOrDefaultAsync(x => x.Id == columnBody.Id, cancellationToken);

                if (column != null)
                {
                    column.Name = columnBody.Name;
                    column.Props = columnBody.Props;

                    await _context.SaveChangesAsync(cancellationToken);

                    return columnBody;
                }

                var columnNode = new Column
                {
                    Id = columnBody.Id != Guid.Empty ? columnBody.Id : Guid.NewGuid(),
                    Name = columnBody.Name,
                    Type = NodeType.Column,
                    Props = columnBody.Props
                };

                columnNode = (await _context.Columns.AddAsync(columnNode, cancellationToken))?.Entity;

                await _context.SaveChangesAsync(cancellationToken);

                return columnBody.ApplyNodeMetadata(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при создании колонки {columnBody.Id}: {ex.Message}");

                throw;
            }

        }

        public async Task<bool> DeleteNode(
            Guid nodeId,
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            var column = await _context.Nodes.FirstOrDefaultAsync(x => x.Id == nodeId, cancellationToken);

            if (column == null) return false;

            _context.Nodes.Remove(column);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<List<ColumnBody>?> CreateOrUpdateColumns(
            List<ColumnBody> columns,
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            var columnNodes = new List<ColumnBody>();

            foreach (var column in columns)
            {
                var addedColumn = await CreateOrUpdateColumn(column, accountId, column, cancellationToken);
                if (addedColumn != null)
                    columnNodes.Add(addedColumn);
            }
            return columnNodes;
        }
    }
}