using Microsoft.EntityFrameworkCore;
using Planer_task_board.Core.Entities.Events;
using Planer_task_board.Core.Entities.Models;
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

        public async Task<BoardColumn?> AddBoardColumn(Board board, string name)
        {
            var boardColumn = new BoardColumn
            {
                Board = board,
                Name = name,
            };

            boardColumn = (await _context.BoardColumns.AddAsync(boardColumn))?.Entity;
            await _context.SaveChangesAsync();

            return boardColumn;
        }
    }
}