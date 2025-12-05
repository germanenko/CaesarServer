using Microsoft.EntityFrameworkCore;
using Planer_task_board.Core.Entities.Events;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using Planer_task_board.Infrastructure.Data;

namespace Planer_task_board.Infrastructure.Repository
{
    public class BoardRepository : IBoardRepository
    {
        private readonly ContentDbContext _context;
        private readonly INotifyService _notifyService;

        public BoardRepository(ContentDbContext context, INotifyService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }

        public async Task<Board?> AddAsync(CreateBoardBody createBoardBody, Guid accountId)
        {
            var board = new Board
            {
                Id = createBoardBody.Id != Guid.Empty ? createBoardBody.Id : Guid.NewGuid(),
                Name = createBoardBody.Name,
                Type = NodeType.Board,
                Props = createBoardBody.Props
            };

            await _context.Boards.AddAsync(board);

            var history = new History
            {
                NodeId = board.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = accountId
            };

            await _context.History.AddAsync(history);

            var publicationStatus = new PublicationStatusModel()
            {
                Id = Guid.NewGuid(),
                Node = board,
                NodeId = board.Id,
                Status = createBoardBody.PublicationStatus,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.PublicationStatuses.AddAsync(publicationStatus);

            var accessRight = new AccessRight()
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                NodeId = board.Id,
                Node = board, 
                AccessType = AccessType.Creator
            };

            await _context.AccessRights.AddAsync(accessRight);

            var nodeLink = new NodeLink()
            {
                Id = Guid.NewGuid(),
                ParentId = board.Id,
                ChildId = board.Id
            };

            await _context.NodeLinks.AddAsync(nodeLink);

            await _context.History.AddAsync(new History()
            {
                NodeId = board.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = accountId
            });

            await _context.NodeLinks.AddAsync(nodeLink);

            await _context.SaveChangesAsync();

            return board;
        }

        public async Task<List<Board>?> AddRangeAsync(List<CreateBoardBody> boards, Guid accountId)
        {
            List<Board> newBoardNodes = new List<Board>();

            foreach (var board in boards)
            {
                newBoardNodes.Add(await AddAsync(board, accountId));
            }

            await _context.Boards.AddRangeAsync(newBoardNodes);
            await _context.SaveChangesAsync();

            return newBoardNodes;
        }

        public async Task<AccessRight?> AddBoardMember(Guid accountId, Guid boardId, AccessType accessType)
        {
            var board = await GetAsync(boardId);
            if (board == null)
                return null;

            var boardMember = await GetBoardMemberAsync(accountId, boardId);
            if (boardMember != null)
                return null;

            boardMember = new AccessRight
            {
                NodeId = boardId,
                AccountId = accountId,
                AccessType = accessType
            };

            boardMember = (await _context.AccessRights.AddAsync(boardMember))?.Entity;
            await _context.SaveChangesAsync();

            var tasksIds = await _context.NodeLinks.Where(e => e.ParentId == boardId)
                .Join(_context.NodeLinks,
                    n1 => n1.ChildId,
                    c => c.ParentId,
                    (n1, c) => c)
                .Join(_context.Nodes,
                    n2 => n2.ChildId,
                    t => t.Id,
                    (n2, t) => t)
                .Select(t => t.Id)
                .ToListAsync();

            var addAccountToTaskChatsEvent = new AddAccountsToTaskChatsEvent
            {
                AccountIds = new List<Guid> { accountId },
                TaskIds = tasksIds,
            };

            _notifyService.Publish(addAccountToTaskChatsEvent, PublishEvent.AddAccountsToTaskChats);
            return boardMember;
        }

        public async Task<AccessRight?> GetBoardMemberAsync(Guid accountId, Guid boardId)
        {
            return await _context.AccessRights.FirstOrDefaultAsync(e => e.NodeId == boardId && e.AccountId == accountId);
        }

        public async Task<IEnumerable<Node>> GetAll(Guid accountId)
        {
            var access = await _context.AccessRights.Where(x => x.AccountId == accountId && x.NodeType == NodeType.Board).Select(x => x.NodeId).ToListAsync();

            var boards = await _context.Nodes.Where(x => access.Contains(x.Id)).ToListAsync();

            return boards;
        }

        public async Task<Node?> GetAsync(Guid id)
            => await _context.Nodes.FindAsync(id);

        public async Task<IEnumerable<Node>> GetBoardColumns(Guid boardId)
        {
            var nodes = await _context.NodeLinks.Where(x => x.ParentId == boardId)
                .Join(_context.Nodes,
                    nl => nl.ChildId,
                    n => n.Id,
                    (nl, n) => n) 
                .Where(x => x.Type == NodeType.Column)
                .Select(x => x.Id)
                .ToListAsync();

            return await _context.Nodes.Where(x => nodes.Contains(x.Id)).ToListAsync();
        }

        public async Task<IEnumerable<Node>> GetAllBoardColumns(Guid accountId)
        {
            var access = await _context.AccessRights
                .Where(x => x.AccountId == accountId && x.NodeType == NodeType.Board)
                .Select(x => x.NodeId)
                .ToListAsync();

            var columnIds = await _context.NodeLinks.Where(x => access.Contains(x.ParentId))
                .Join(_context.Nodes,
                    nl => nl.ChildId,
                    n => n.Id,
                    (nl, n) => n)
                .Where(x => x.Type == NodeType.Column)
                .Select(x => x.Id)
                .ToListAsync();

            var columns = await _context.Nodes.Where(x => columnIds.Contains(x.Id)).ToListAsync();

            return columns;
        }

        public async Task<Node?> GetBoardColumn(Guid columnId)
        {
            return await _context.Nodes
                .FirstOrDefaultAsync(e => e.Id == columnId);
        }
        public async Task<Node?> GetBoardColumnByChild(Guid childId)
        {
            var link = await _context.NodeLinks
                .FirstOrDefaultAsync(e => e.ChildId == childId && e.RelationType == RelationType.Contains);

            var node = await _context.Nodes.FirstOrDefaultAsync(x => x.Id == link.ParentId);

            return node;
        }


        public async Task<Node?> GetBoard(Guid columnId)
        {
            return await _context.NodeLinks
                .Where(e => e.ChildId == columnId)
                .Join(_context.Nodes,
                n => n.ParentId,
                b => b.Id,
                (n, b) => b).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Guid>> GetBoardMembers(Guid boardId, int count, int offset)
        {
            var members = await _context.AccessRights
                .Where(x => x.NodeId == boardId)
                .Select(x => x.AccountId.Value)
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            return members;
        }

        public async Task<IEnumerable<AccessRight>> GetBoardMembers(IEnumerable<Guid?> memberIds, Guid boardId)
        {
            return await _context.AccessRights
                .Where(e => e.NodeId == boardId && memberIds.Contains(e.AccountId))
                .ToListAsync();
        }

        public async Task<Column?> AddBoardColumn(CreateColumnBody column, Guid accountId)
        {
            var columnNode = new Column
            {
                Id = column.Id,
                Name = column.Name
            };
            
            await _context.History.AddAsync(new History
            {
                NodeId = column.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = accountId
            });

            await _context.PublicationStatuses.AddAsync(new PublicationStatusModel()
            {
                Node = columnNode,
                NodeId = column.Id,
                Status = column.PublicationStatus,
                UpdatedAt = column.UpdatedAt
            });

            await _context.NodeLinks.AddAsync(new NodeLink()
            {
                ParentId = columnNode.Id,
                ChildId = columnNode.Id
            });

            columnNode = (await _context.Columns.AddAsync(columnNode))?.Entity;

            await _context.SaveChangesAsync();

            return columnNode;
        }

        public async Task<List<Column>?> AddBoardColumns(List<CreateColumnBody> columns, Guid accountId)
        {
            var columnNodes = new List<Column>();
            var statuses = new List<PublicationStatusModel>();
            var links = new List<NodeLink>();
            var histories = new List<History>();

            foreach (var column in columns)
            {
                columnNodes.Add(new Column
                {
                    Id = column.Id,
                    Name = column.Name
                });

                statuses.Add(new PublicationStatusModel()
                {
                    NodeId = column.Id,
                    Status = column.PublicationStatus,
                    UpdatedAt = column.UpdatedAt
                });

                links.Add(new NodeLink()
                {
                    ParentId = column.Id,
                    ChildId = column.Id
                });
                histories.Add(new History()
                {
                    NodeId = column.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = accountId
                });
            }

            await _context.Columns.AddRangeAsync(columnNodes);
            await _context.PublicationStatuses.AddRangeAsync(statuses);
            await _context.NodeLinks.AddRangeAsync(links);
            await _context.History.AddRangeAsync(histories);

            await _context.SaveChangesAsync();

            return columnNodes;
        }
    }
}