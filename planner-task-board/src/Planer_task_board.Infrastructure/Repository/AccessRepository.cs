using Microsoft.EntityFrameworkCore;
using Npgsql;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Request;
using Planer_task_board.Core.Enums;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Infrastructure.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using static NpgsqlTypes.NpgsqlTsQuery;

namespace Planer_task_board.Infrastructure.Repository
{
    public class AccessRepository : IAccessRepository
    {
        private readonly ContentDbContext _context;

        public AccessRepository(ContentDbContext context)
        {
            _context = context;
        }

        public async Task<Node?> CreateOrUpdateGroup(Guid accountId, CreateAccessGroupBody body)
        {
            var hasAccess = await CheckAccess(accountId, body.BoardId);

            if (hasAccess)
            {
                var newNodes = new List<Node>();
                var newNodeLinks = new List<NodeLink>();

                var group = new AccessGroup()
                {
                    Id = body.Id,
                    Name = body.Name
                };

                newNodes.Add(new Node()
                {
                    Id = body.Id,
                    Name = body.Name,
                    CreatedBy = accountId,
                    CreatedAt = DateTime.UtcNow,
                    Type = NodeType.AccessGroup,
                    Props = JsonSerializer.Serialize(group)
                });

                newNodeLinks.Add(new NodeLink()
                {
                    ParentId = body.BoardId,
                    ChildId = body.Id,
                    ParentType = NodeType.Board,
                    ChildType = NodeType.AccessGroup
                });

                foreach (var userId in body.UserIds)
                {
                    var member = new AccessGroupMember()
                    {
                        AccessGroupId = group.Id,
                        JoinedAt = DateTime.UtcNow,
                        AccountId = userId,
                    };

                    newNodes.Add(new Node()
                    {
                        Id = member.Id,
                        CreatedBy = accountId,
                        CreatedAt = DateTime.UtcNow,
                        Type = NodeType.AccessGroupMember,
                        Props = JsonSerializer.Serialize(member)
                    });

                    newNodeLinks.Add(new NodeLink()
                    {
                        ParentId = group.Id,
                        ChildId = member.Id,
                        ParentType = NodeType.AccessGroup,
                        ChildType = NodeType.AccessGroupMember
                    });

                }

                await _context.Nodes.AddRangeAsync(newNodes);
                await _context.NodeLinks.AddRangeAsync(newNodeLinks);

                await _context.SaveChangesAsync();
            }

            return null;
        }

        public async Task<Node?> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId)
        {
            var board = await _context.NodeLinks.Where(x => x.ChildId == groupId).FirstOrDefaultAsync();

            var hasAccess = await CheckAccess(accountId, board.ParentId);

            if (hasAccess)
            { 
                var groupNode = await _context.Nodes.Where(x => x.Id == groupId).FirstOrDefaultAsync();

                var group = JsonSerializer.Deserialize<AccessGroup>(groupNode.Props);

                var member = new AccessGroupMember()
                {
                    AccessGroupId = group.Id,
                    JoinedAt = DateTime.UtcNow,
                    AccountId = userToAdd,
                };

                group.Members.Add(member);

                groupNode.Props = JsonSerializer.Serialize(group);

                await _context.SaveChangesAsync();

                return groupNode;
            }

            return null;
        }

        public async Task<bool> CheckAccess(Guid accountId, Guid nodeId)
        {
            var creator = await _context.Nodes.AnyAsync(x => x.CreatedBy == accountId && x.Id == nodeId);

            if (creator)
                return true;

            var directAccess = await _context.AccessRights.AnyAsync(x => x.NodeId == nodeId && x.AccountId == accountId);

            if (directAccess)
                return true;

            var userGroupAccess = await _context.AccessRights
                .Where(ar => ar.NodeId == nodeId && ar.IsGroupAccess)
                .Select(ar => ar.AccessGroupId.Value)
                .Join(_context.NodeLinks,
                    groupId => groupId,
                    nodeLink => nodeLink.ParentId,
                    (groupId, nodeLink) => nodeLink)
                .Where(nl => nl.ChildNode.Type == NodeType.AccessGroup)
                .Select(nl => nl.ChildNode.Props)
                .AnyAsync(props => CheckIfUserInGroup(props, accountId));

            return userGroupAccess;
        }

        private bool CheckIfUserInGroup(string? props, Guid accountId)
        {
            if (string.IsNullOrEmpty(props))
                return false;

            try
            {
                var memberIds = JsonSerializer.Deserialize<List<Guid>>(props);
                return memberIds?.Contains(accountId) == true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Node?> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId)
        {
            var board = await _context.NodeLinks.Where(x => x.ChildId == groupId).FirstOrDefaultAsync();

            var hasAccess = await CheckAccess(accountId, board.ParentId);

            if (hasAccess)
            {
                var groupNode = await _context.Nodes.Where(x => x.Id == groupId).FirstOrDefaultAsync();

                var group = JsonSerializer.Deserialize<AccessGroup>(groupNode.Props);

                var member = group.Members.Where(x => x.AccountId == accountId).FirstOrDefault();

                group.Members.Remove(member);

                groupNode.Props = JsonSerializer.Serialize(group);

                await _context.SaveChangesAsync();

                return groupNode;
            }

            return null;
        }
    }
}