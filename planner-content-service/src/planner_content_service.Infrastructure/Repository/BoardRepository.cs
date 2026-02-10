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


        public async Task<Node?> GetAsync(Guid id)
            => await _context.Nodes.FindAsync(id);

        public async Task<IEnumerable<Node>> GetBoardColumns(List<Guid> columnIds)
        {
            return await _context.Nodes.Where(x => columnIds.Contains(x.Id)).ToListAsync();
        }


        public async Task<Node?> GetBoardColumn(Guid columnId)
        {
            return await _context.Nodes
                .FirstOrDefaultAsync(e => e.Id == columnId);
        }


        public async Task<Node?> GetBoard(Guid boardId)
        {
            return await _context.Nodes
                .Where(e => e.Id == boardId).FirstOrDefaultAsync();
        }

        public async Task<Column?> AddBoardColumn(ColumnBody column, Guid accountId)
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

                return columnNode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при создании колонки {column.Id}: {ex.Message}");

                throw;
            }

        }

        public async Task<List<Column>?> AddBoardColumns(List<ColumnBody> columns, Guid accountId)
        {
            var columnNodes = new List<Column>();
            //var statuses = new List<PublicationStatusModel>();
            //var links = new List<NodeLink>();

            foreach (var column in columns)
            {
                var addedColumn = await AddBoardColumn(column, accountId);
                if (addedColumn != null)
                    columnNodes.Add(addedColumn);

                //columnNodes.Add(new Column
                //{
                //    Id = column.Id,
                //    Name = column.Name
                //});

                //statuses.Add(new PublicationStatusModel()
                //{
                //    NodeId = column.Id,
                //    Status = column.PublicationStatus,
                //    UpdatedAt = column.UpdatedAt
                //});

                //links.Add(new NodeLink()
                //{
                //    ParentId = column.Id,
                //    ChildId = column.Id
                //});
            }
            return columnNodes;

            //try
            //{
            //    await _context.Columns.AddRangeAsync(columnNodes);
            //    await _context.PublicationStatuses.AddRangeAsync(statuses);
            //    await _context.NodeLinks.AddRangeAsync(links);

            //    await _context.SaveChangesAsync();

            //    foreach (var column in columns)
            //    {
            //        CreateColumnEvent columnEvent = new CreateColumnEvent()
            //        {
            //            Column = BodyConverter.ClientToServerBody(column),
            //            CreatorId = accountId
            //        };

            //        _ = Task.Run(() => _notifyService.Publish(columnEvent, PublishEvent.CreateColumn));
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Ошибка при создании колонок: {ex.Message}");

            //    throw;
            //}

        }
    }
}