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

        public async Task<Node?> AddAsync(CreateBoardBody createBoardBody, Guid accountId)
        {
            var boardNode = new Node
            {
                Id = createBoardBody.Id,
                Name = createBoardBody.Name,
                UpdatedAt = createBoardBody.UpdatedAt,
                CreatedAt = createBoardBody.UpdatedAt,
                CreatedBy = accountId
            };

            await _context.PublicationStatuses.AddAsync(new PublicationStatusModel()
            {
                Node = boardNode,
                NodeId = boardNode.Id,
                Status = PublicationStatus.Active,
                UpdatedAt = boardNode.UpdatedAt
            });

            await _context.AccessRights.AddAsync(new AccessRight()
            {
                AccountId = accountId,
                NodeId = boardNode.Id,
                AccessType = AccessType.Creator
            });


            boardNode = (await _context.Nodes.AddAsync(boardNode))?.Entity;
            await _context.SaveChangesAsync();

            return boardNode;
        }

        public async Task<List<Node>?> AddRangeAsync(List<CreateBoardBody> boards, Guid accountId)
        {
            List<Node> newBoardNodes = new List<Node>();

            foreach (var board in boards)
            {
                newBoardNodes.Add(await AddAsync(board, accountId));
            }

            await _context.Nodes.AddRangeAsync(newBoardNodes);
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
            var access = await _context.AccessRights.Where(x => x.AccountId == accountId && x.ResourceType == NodeType.Board).Select(x => x.NodeId).ToListAsync();

            var boards = await _context.Nodes.Where(x => access.Contains(x.Id)).ToListAsync();

            return boards;
        }

        public async Task<Node?> GetAsync(Guid id)
            => await _context.Nodes.FindAsync(id);

        public async Task<IEnumerable<Node>> GetBoardColumns(Guid boardId)
        {
            var nodes = await _context.NodeLinks.Where(x => x.ParentId == boardId && x.ChildType == NodeType.Column).Select(x => x.ChildId).ToListAsync();

            return await _context.Nodes.Where(x => nodes.Contains(x.Id)).ToListAsync();
        }

        public async Task<IEnumerable<Node>> GetAllBoardColumns(Guid accountId)
        {
            var access = await _context.AccessRights
                .Where(x => x.AccountId == accountId && x.ResourceType == NodeType.Board)
                .Select(x => x.NodeId)
                .ToListAsync();

            var columnIds = await _context.NodeLinks.Where(x => access.Contains(x.ParentId) && x.ChildType == NodeType.Column).Select(x => x.ChildId).ToListAsync();

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
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            return members.Select(e => e.AccountId);
        }

        public async Task<IEnumerable<AccessRight>> GetBoardMembers(IEnumerable<Guid> memberIds, Guid boardId)
        {
            return await _context.AccessRights
                .Where(e => e.NodeId == boardId && memberIds.Contains(e.AccountId))
                .ToListAsync();
        }

        public async Task<Node?> AddBoardColumn(CreateColumnBody column, Guid accountId)
        {
            var columnNode = new Node
            {
                Id = column.Id,
                Name = column.Name,
                UpdatedAt = column.UpdatedAt,
                CreatedBy = accountId,
                CreatedAt = column.UpdatedAt
            };

            columnNode = (await _context.Nodes.AddAsync(columnNode))?.Entity;

            await _context.SaveChangesAsync();

            return columnNode;
        }

        public async Task<List<Node>?> AddBoardColumns(List<CreateColumnBody> columns, Guid accountId)
        {
            var columnNodes = new List<Node>();

            foreach (var column in columns)
            {
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

            await _context.Nodes.AddRangeAsync(columnNodes);

            await _context.SaveChangesAsync();

            return columnNodes;
        }
    }
}