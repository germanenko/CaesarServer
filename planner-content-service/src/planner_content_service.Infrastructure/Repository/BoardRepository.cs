using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_content_service.Core.Entities.Models;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_content_service.Infrastructure.Data;
using planner_server_package.Converters;
using planner_server_package.Events;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using System.Diagnostics;

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

        public async Task<BoardBody?> CreateOrUpdateBoardAsync(BoardBody createBoardBody, Guid accountId)
        {
            try
            {
                var board = await _context.Boards.FirstOrDefaultAsync(x => x.Id == createBoardBody.Id);

                if (board != null)
                {
                    board.Name = createBoardBody.Name;
                    board.Props = createBoardBody.Props;

                    await _context.SaveChangesAsync();

                    return createBoardBody;
                }

                var newBoard = new Board
                {
                    Id = createBoardBody.Id != Guid.Empty ? createBoardBody.Id : Guid.NewGuid(),
                    Name = createBoardBody.Name,
                    Type = NodeType.Board,
                    Props = createBoardBody.Props
                };

                await _context.Boards.AddAsync(newBoard);

                await _context.SaveChangesAsync();

                return createBoardBody;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании доски {createBoardBody.Name}: {ex.Message}");

                throw;
            }
        }

        public async Task<List<BoardBody>?> CreateOrUpdateBoards(List<BoardBody> boards, Guid accountId)
        {
            List<BoardBody> newBoardNodes = new List<BoardBody>();

            foreach (var board in boards)
            {
                newBoardNodes.Add(await CreateOrUpdateBoardAsync(board, accountId));
            }

            await _context.SaveChangesAsync();

            return newBoardNodes;
        }


        public async Task<ColumnBody?> CreateOrUpdateColumn(ColumnBody columnBody, Guid accountId)
        {
            try
            {
                var column = await _context.Columns.FirstOrDefaultAsync(x => x.Id == columnBody.Id);

                if (column != null)
                {
                    column.Name = columnBody.Name;
                    column.Props = columnBody.Props;

                    await _context.SaveChangesAsync();

                    return columnBody;
                }

                var columnNode = new Column
                {
                    Id = columnBody.Id != Guid.Empty ? columnBody.Id : Guid.NewGuid(),
                    Name = columnBody.Name,
                    Type = NodeType.Column,
                    Props = columnBody.Props
                };

                columnNode = (await _context.Columns.AddAsync(columnNode))?.Entity;

                await _context.SaveChangesAsync();

                return columnBody;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при создании колонки {columnBody.Id}: {ex.Message}");

                throw;
            }

        }

        public async Task<List<ColumnBody>?> CreateOtUpdateColumns(List<ColumnBody> columns, Guid accountId)
        {
            var columnNodes = new List<ColumnBody>();

            foreach (var column in columns)
            {
                var addedColumn = await CreateOrUpdateColumn(column, accountId);
                if (addedColumn != null)
                    columnNodes.Add(addedColumn);
            }
            return columnNodes;
        }
    }
}