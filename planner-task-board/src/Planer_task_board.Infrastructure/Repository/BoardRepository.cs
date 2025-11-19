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
                UpdatedAt = createBoardBody.UpdatedAt,
                Members = new List<BoardMember>
                {
                    new() {
                        Role = BoardMemberRoles.Admin.ToString(),
                        AccountId = accountId
                    }
                }
            };

            board = (await _context.Boards.AddAsync(board))?.Entity;
            await _context.SaveChangesAsync();

            return board;
        }

        public async Task<List<Board>?> AddRangeAsync(List<CreateBoardBody> boards, Guid accountId)
        {
            List<Board> newBoards = new List<Board>();
            foreach (var board in boards)
            {
                var newBoard = new Board()
                {
                    Id = board.Id,
                    Name = board.Name,
                    UpdatedAt = board.UpdatedAt,
                    Members = new List<BoardMember>
                    {
                        new() {
                            Role = BoardMemberRoles.Admin.ToString(),
                            AccountId = accountId
                        }
                    }
                };
                newBoards.Add(newBoard);
            }
            await _context.Boards.AddRangeAsync(newBoards);
            await _context.SaveChangesAsync();

            return newBoards;
        }

        public async Task<BoardMember?> AddBoardMember(Guid accountId, Guid boardId)
        {
            var board = await GetAsync(boardId);
            if (board == null)
                return null;

            var boardMember = await GetBoardMemberAsync(accountId, boardId);
            if (boardMember != null)
                return null;

            boardMember = new BoardMember
            {
                Board = board,
                AccountId = accountId,
                Role = BoardMemberRoles.Participant.ToString(),
            };

            boardMember = (await _context.BoardMembers.AddAsync(boardMember))?.Entity;
            await _context.SaveChangesAsync();

            var columnIds = await _context.Nodes.Where(e => e.ParentId == boardId)
                .Select(e => e.Id)
                .ToListAsync();

            var taskIds = await _context.BoardColumnTasks
                .Where(e => columnIds.Contains(e.ColumnId))
                .Select(e => e.TaskId)
                .Distinct()
                .ToListAsync();

            var addAccountToTaskChatsEvent = new AddAccountsToTaskChatsEvent
            {
                AccountIds = new List<Guid> { accountId },
                TaskIds = taskIds,
            };

            _notifyService.Publish(addAccountToTaskChatsEvent, PublishEvent.AddAccountsToTaskChats);
            return boardMember;
        }

        public async Task<BoardMember?> GetBoardMemberAsync(Guid accountId, Guid boardId)
            => await _context.BoardMembers
                .FirstOrDefaultAsync(e => e.BoardId == boardId && e.AccountId == accountId);

        public async Task<BoardColumnMember?> GetColumnMemberAsync(Guid accountId, Guid? columnId)
            => await _context.BoardColumnMembers
                .FirstOrDefaultAsync(e => e.ColumnId == columnId && e.AccountId == accountId);

        public async Task<IEnumerable<Board>> GetAll(Guid accountId)
        {
            var availableMemberBoards = await _context.BoardMembers
                .Include(e => e.Board)
                .Where(e => e.AccountId == accountId)
                .ToListAsync();

            return availableMemberBoards.Select(e => e.Board);
        }

        public async Task<Board?> GetAsync(Guid id)
            => await _context.Boards.FindAsync(id);

        public async Task<IEnumerable<BoardColumn>> GetBoardColumns(Guid boardId)
        {
            var nodes = await _context.Nodes.Where(x => x.ParentId == boardId).Select(x => x.ChildId).ToListAsync();

            return await _context.BoardColumns.Where(x => nodes.Contains(x.Id)).ToListAsync();

            //return await _context.BoardColumns
            //    .Where(e => e.BoardId == boardId)
            //    .ToListAsync();
        }

        public async Task<IEnumerable<BoardColumn>> GetAllBoardColumns(Guid accountId)
        {
            var availableMemberBoardColumns = await _context.BoardColumnMembers
                //.Include(e => e.Column)
                .Where(e => e.AccountId == accountId)
                .Select(x => x.ColumnId)
                .ToListAsync();

            var columns = await _context.BoardColumns.Where(x => availableMemberBoardColumns.Contains(x.Id)).ToListAsync();

            return columns;
        }

        public async Task<BoardColumn?> GetBoardColumn(Guid? columnId)
        {
            return await _context.BoardColumns
                .FirstOrDefaultAsync(e => e.Id == columnId);
        }

        public async Task<IEnumerable<Guid>> GetBoardMembers(Guid boardId, int count, int offset)
        {
            var boardMembers = await _context.BoardMembers
                .OrderBy(e => e.AccountId)
                .Where(e => e.BoardId == boardId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            return boardMembers.Select(e => e.AccountId);
        }

        public async Task<IEnumerable<BoardMember>> GetBoardMembers(IEnumerable<Guid> memberIds, Guid boardId)
        {
            return await _context.BoardMembers
                .Where(e => e.BoardId == boardId && memberIds.Contains(e.AccountId))
                .ToListAsync();
        }

        public async Task<BoardColumn?> AddBoardColumn(Board board, CreateColumnBody column, Guid accountId)
        {
            var boardColumn = new BoardColumn
            {
                Id = column.Id,
                Name = column.Name,
                UpdatedAt = column.UpdatedAt
            };

            boardColumn = (await _context.BoardColumns.AddAsync(boardColumn))?.Entity;

            //var boardColumnMember = new BoardColumnMember()
            //{
            //    Column = boardColumn,
            //    ColumnId = boardColumn.Id,
            //    AccountId = accountId,
            //    Role = "Admin"
            //};
            //await _context.BoardColumnMembers.AddAsync(boardColumnMember);

            var node = new Node()
            {
                ParentId = board.Id,
                ChildId = column.Id,
                RelationType = RelationType.Contains
            };
            await _context.Nodes.AddAsync(node);

            await _context.SaveChangesAsync();

            return boardColumn;
        }

        public async Task<List<BoardColumn>?> AddBoardColumns(List<CreateColumnBody> columns, Guid accountId)
        {
            var boardColumns = new List<BoardColumn>();

            //var boardColumnMembers = new List<BoardColumnMember>();

            var nodes = new List<Node>();

            foreach (var column in columns)
            {
                BoardColumn newColumn = new BoardColumn()
                {
                    Id = column.Id,
                    Name = column.Name,
                };
                boardColumns.Add(newColumn);

                
                nodes.Add(new Node()
                {
                    ParentId = column.BoardId,
                    ChildId = column.Id,
                    RelationType = RelationType.Contains
                });

                //boardColumnMembers.Add(new BoardColumnMember()
                //{
                //    Column = newColumn,
                //    ColumnId = newColumn.Id,
                //    AccountId = accountId,
                //    Role = "Admin"
                //});
            }

            await _context.BoardColumns.AddRangeAsync(boardColumns);

            //await _context.BoardColumnMembers.AddRangeAsync(boardColumnMembers);
            await _context.Nodes.AddRangeAsync(nodes);

            await _context.SaveChangesAsync();

            return boardColumns;
        }
    }
}