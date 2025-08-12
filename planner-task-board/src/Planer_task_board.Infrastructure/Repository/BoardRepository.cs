using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        public async Task<Board?> AddAsync(string boardName, Guid accountId)
        {
            var board = new Board
            {
                Name = boardName,
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

        public async Task<List<Board>?> AddRangeAsync(List<string> boardNames, Guid accountId)
        {
            List<Board> boards = new List<Board>();
            foreach (var boardName in boardNames)
            {
                var board = new Board()
                {
                    Name = boardName,
                    Members = new List<BoardMember>
                    {
                        new() {
                            Role = BoardMemberRoles.Admin.ToString(),
                            AccountId = accountId
                        }
                    }
                };
                boards.Add(board);
            }
            await _context.Boards.AddRangeAsync(boards);
            await _context.SaveChangesAsync();

            return boards;
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

            var columnIds = await _context.BoardColumns.Where(e => e.BoardId == boardId)
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

        public async Task<BoardColumnMember?> GetColumnMemberAsync(Guid accountId, Guid columnId)
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
            return await _context.BoardColumns
                .Where(e => e.BoardId == boardId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BoardColumn>> GetAllBoardColumns(Guid accountId)
        {
            var availableMemberBoardColumns = await _context.BoardColumnMembers
                .Include(e => e.Column)
                .Where(e => e.AccountId == accountId)
                .ToListAsync();

            return availableMemberBoardColumns.Select(e => e.Column);
        }

        public async Task<BoardColumn?> GetBoardColumn(Guid columnId)
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

        public async Task<BoardColumn?> AddBoardColumn(Board board, string name, Guid accountId)
        {
            var boardColumn = new BoardColumn
            {
                Board = board,
                Name = name,
            };

            boardColumn = (await _context.BoardColumns.AddAsync(boardColumn))?.Entity;

            var boardColumnMember = new BoardColumnMember()
            {
                Column = boardColumn,
                ColumnId = boardColumn.Id,
                AccountId = accountId
            };
            await _context.BoardColumnMembers.AddAsync(boardColumnMember);

            await _context.SaveChangesAsync();

            return boardColumn;
        }

        public async Task<List<BoardColumn>?> AddBoardColumns(List<CreateColumnBody> columns, Guid accountId)
        {
            var boardColumns = new List<BoardColumn>();
            var boardColumnMembers = new List<BoardColumnMember>();

            foreach (var column in columns)
            {
                BoardColumn newColumn = new BoardColumn()
                {
                    Board = _context.Boards.Where(b => b.Id == column.BoardId).First(),
                    Name = column.Name,
                };
                boardColumns.Add(newColumn);

                boardColumnMembers.Add(new BoardColumnMember()
                {
                    Column = newColumn,
                    ColumnId = newColumn.Id,
                    AccountId = accountId,
                    Role = "Common"
                });
            }

            await _context.BoardColumns.AddRangeAsync(boardColumns);

            await _context.BoardColumnMembers.AddRangeAsync(boardColumnMembers);

            await _context.SaveChangesAsync();

            return boardColumns;
        }
    }
}