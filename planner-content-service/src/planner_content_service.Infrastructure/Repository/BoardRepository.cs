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
using System.Diagnostics;

namespace planner_content_service.Infrastructure.Repository
{
    public class BoardRepository : IBoardRepository
    {
        private readonly ContentDbContext _context;
        private readonly INotifyService _notifyService;
        private readonly ILogger<BoardRepository> _logger;

        public BoardRepository(ContentDbContext context, INotifyService notifyService, ILogger<BoardRepository> logger)
        {
            _context = context;
            _notifyService = notifyService;
            _logger = logger;
        }

        public async Task<BoardBody?> AddAsync(BoardBody createBoardBody, Guid accountId)
        {
            try
            {
                var board = new Board
                {
                    Id = createBoardBody.Id != Guid.Empty ? createBoardBody.Id : Guid.NewGuid(),
                    Name = createBoardBody.Name,
                    Type = NodeType.Board,
                    Props = createBoardBody.Props
                };

                await _context.Boards.AddAsync(board);

                await _context.SaveChangesAsync();

                return createBoardBody;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании доски {createBoardBody.Name}: {ex.Message}");

                throw;
            }
        }

        public async Task<List<BoardBody>?> AddRangeAsync(List<BoardBody> boards, Guid accountId)
        {
            List<BoardBody> newBoardNodes = new List<BoardBody>();

            foreach (var board in boards)
            {
                newBoardNodes.Add(await AddAsync(board, accountId));
            }

            await _context.SaveChangesAsync();

            return newBoardNodes;
        }


        public async Task<ColumnBody?> AddBoardColumn(ColumnBody column, Guid accountId)
        {
            try
            {
                var columnNode = new Column
                {
                    Id = column.Id != Guid.Empty ? column.Id : Guid.NewGuid(),
                    Name = column.Name,
                    Type = NodeType.Column
                };

                columnNode = (await _context.Columns.AddAsync(columnNode))?.Entity;

                await _context.SaveChangesAsync();

                return column;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при создании колонки {column.Id}: {ex.Message}");

                throw;
            }

        }

        public async Task<List<ColumnBody>?> AddBoardColumns(List<ColumnBody> columns, Guid accountId)
        {
            var columnNodes = new List<ColumnBody>();

            foreach (var column in columns)
            {
                var addedColumn = await AddBoardColumn(column, accountId);
                if (addedColumn != null)
                    columnNodes.Add(addedColumn);
            }
            return columnNodes;
        }
    }
}