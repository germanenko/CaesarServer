using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Planer_task_board.Core.Entities.Events;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using Planer_task_board.Infrastructure.Data;
using System.Xml.Linq;

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
                Id = createBoardBody.Id,
                Name = createBoardBody.Name,
                UpdatedAt = createBoardBody.UpdatedAt
            };

            var boardNode = new Node
            {
                Id = createBoardBody.Id,
                Name = createBoardBody.Name,
                UpdatedAt = createBoardBody.UpdatedAt,
                CreatedAt = createBoardBody.UpdatedAt,
                CreatedBy = accountId
            };

            await _context.AccessRights.AddAsync(new AccessRight()
            {
                AccountId = accountId,
                ResourceId = board.Id,
                AccessType = AccessType.Creator
            });


            board = (await _context.Boards.AddAsync(board))?.Entity;
            boardNode = (await _context.Nodes.AddAsync(boardNode))?.Entity;
            await _context.SaveChangesAsync();

            return board;
        }

        public async Task<List<Board>?> AddRangeAsync(List<CreateBoardBody> boards, Guid accountId)
        {
            List<Board> newBoards = new List<Board>();
            List<Node> newBoardNodes = new List<Node>();
            foreach (var board in boards)
            {
                var newBoard = new Board()
                {
                    Id = board.Id,
                    Name = board.Name,
                    UpdatedAt = board.UpdatedAt
                };
                newBoards.Add(newBoard);

                var boardNode = new Node
                {
                    Id = board.Id,
                    Name = board.Name,
                    UpdatedAt = board.UpdatedAt,
                    CreatedAt = board.UpdatedAt,
                    CreatedBy = accountId
                };
                newBoardNodes.Add(boardNode);

                await _context.AccessRights.AddAsync(new AccessRight()
                {
                    AccountId = accountId,
                    ResourceId = newBoard.Id,
                    AccessType = AccessType.Creator
                });
            }
            await _context.Boards.AddRangeAsync(newBoards);
            await _context.Nodes.AddRangeAsync(newBoardNodes);
            await _context.SaveChangesAsync();

            return newBoards;
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
                ResourceId = boardId,
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
                .Join(_context.Tasks,
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
            return await _context.AccessRights.FirstOrDefaultAsync(e => e.ResourceId == boardId && e.AccountId == accountId);
        }

        public async Task<IEnumerable<Board>> GetAll(Guid accountId)
        {
            var access = await _context.AccessRights.Where(x => x.AccountId == accountId && x.ResourceType == NodeType.Board).Select(x => x.ResourceId).ToListAsync();

            var boards = await _context.Boards.Where(x => access.Contains(x.Id)).ToListAsync();

            return boards;
        }

        public async Task<Board?> GetAsync(Guid id)
            => await _context.Boards.FindAsync(id);

        public async Task<IEnumerable<BoardColumn>> GetBoardColumns(Guid boardId)
        {
            var nodes = await _context.NodeLinks.Where(x => x.ParentId == boardId && x.ChildType == NodeType.Column).Select(x => x.ChildId).ToListAsync();

            return await _context.BoardColumns.Where(x => nodes.Contains(x.Id)).ToListAsync();
        }

        public async Task<IEnumerable<BoardColumn>> GetAllBoardColumns(Guid accountId)
        {
            var access = await _context.AccessRights
                .Where(x => x.AccountId == accountId && x.ResourceType == NodeType.Board)
                .Select(x => x.ResourceId)
                .ToListAsync();

            var columnIds = await _context.NodeLinks.Where(x => access.Contains(x.ParentId) && x.ChildType == NodeType.Column).Select(x => x.ChildId).ToListAsync();

            var columns = await _context.BoardColumns.Where(x => columnIds.Contains(x.Id)).ToListAsync();

            return columns;
        }

        public async Task<BoardColumn?> GetBoardColumn(Guid? columnId)
        {
            return await _context.BoardColumns
                .FirstOrDefaultAsync(e => e.Id == columnId);
        }

        public async Task<Board?> GetBoard(Guid columnId)
        {
            return await _context.NodeLinks
                .Where(e => e.ChildId == columnId)
                .Join(_context.Boards,
                n => n.ParentId,
                b => b.Id,
                (n, b) => b).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Guid>> GetBoardMembers(Guid boardId, int count, int offset)
        {
            var members = await _context.AccessRights
                .Where(x => x.ResourceId == boardId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            return members.Select(e => e.AccountId);
        }

        public async Task<IEnumerable<AccessRight>> GetBoardMembers(IEnumerable<Guid> memberIds, Guid boardId)
        {
            return await _context.AccessRights
                .Where(e => e.ResourceId == boardId && memberIds.Contains(e.AccountId))
                .ToListAsync();
        }

        public async Task<BoardColumn?> AddBoardColumn(CreateColumnBody column, Guid accountId)
        {
            var boardColumn = new BoardColumn
            {
                Id = column.Id,
                Name = column.Name,
                UpdatedAt = column.UpdatedAt
            };

            var columnNode = new Node
            {
                Id = column.Id,
                Name = column.Name,
                UpdatedAt = column.UpdatedAt,
                CreatedBy = accountId,
                CreatedAt = column.UpdatedAt
            };

            boardColumn = (await _context.BoardColumns.AddAsync(boardColumn))?.Entity;
            columnNode = (await _context.Nodes.AddAsync(columnNode))?.Entity;

            await _context.SaveChangesAsync();

            return boardColumn;
        }

        public async Task<List<BoardColumn>?> AddBoardColumns(List<CreateColumnBody> columns, Guid accountId)
        {
            var boardColumns = new List<BoardColumn>();
            var columnNodes = new List<Node>();

            foreach (var column in columns)
            {
                BoardColumn newColumn = new BoardColumn()
                {
                    Id = column.Id,
                    Name = column.Name,
                };
                boardColumns.Add(newColumn);

                var columnNode = new Node
                {
                    Id = column.Id,
                    Name = column.Name,
                    UpdatedAt = column.UpdatedAt,
                    CreatedBy = accountId,
                    CreatedAt = column.UpdatedAt
                };
                columnNodes.Add(columnNode);
            }

            await _context.BoardColumns.AddRangeAsync(boardColumns);
            await _context.Nodes.AddRangeAsync(columnNodes);

            await _context.SaveChangesAsync();

            return boardColumns;
        }
    }
}