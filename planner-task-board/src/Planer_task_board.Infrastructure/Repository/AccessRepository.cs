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

        public async Task<AccessGroup?> CreateGroup(Guid accountId, CreateAccessGroupBody body)
        {
            var hasAccess = await CheckAccess(accountId, body.BoardId);
            if (!hasAccess)
                return null;

            var group = new AccessGroup()
            {
                Id = Guid.NewGuid(),
                Name = body.Name
            };

            await _context.AccessGroups.AddAsync(group);

            var members = body.UserIds.Select(userId => new AccessGroupMember()
            {
                AccessGroupId = group.Id,
                AccountId = userId,
                JoinedAt = DateTime.UtcNow
            }).ToList();

            await _context.AccessGroupMembers.AddRangeAsync(members);

            var accessRight = new AccessRight()
            {
                NodeId = body.BoardId,
                AccessGroupId = group.Id
            };
            await _context.AccessRights.AddAsync(accessRight);

            await _context.SaveChangesAsync();

            group.Members = members;
            return group;
        }

        public async Task<AccessGroupMember?> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId)
        {
            var accessRight = await _context.AccessRights
                .FirstOrDefaultAsync(x => x.AccessGroupId == groupId);

            if (accessRight == null)
                return null;

            var hasAccess = await CheckAccess(accountId, accessRight.NodeId);
            if (!hasAccess)
                return null;

            var existingMember = await _context.AccessGroupMembers
                .FirstOrDefaultAsync(x => x.AccessGroupId == groupId && x.AccountId == userToAdd);

            if (existingMember != null)
                return existingMember;

            var member = new AccessGroupMember()
            {
                AccessGroupId = groupId,
                AccountId = userToAdd,
                JoinedAt = DateTime.UtcNow
            };

            await _context.AccessGroupMembers.AddAsync(member);
            await _context.SaveChangesAsync();

            return member;
        }

        public async Task<bool> CheckAccess(Guid accountId, Guid nodeId)
        {
            var isCreator = await _context.History
                .AnyAsync(n => n.Id == nodeId && n.CreatedBy == accountId);

            if (isCreator)
                return true;

            return await _context.AccessRights
                .Where(ar => ar.NodeId == nodeId)
                .AnyAsync(ar =>
                    (!ar.IsGroupAccess && ar.AccountId == accountId) ||
                    (ar.IsGroupAccess && ar.AccessGroup.Members.Any(m => m.AccountId == accountId)));
        }


        public async Task<AccessGroupMember?> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId)
        {
            var accessRight = await _context.AccessRights
                .FirstOrDefaultAsync(x => x.AccessGroupId == groupId);

            if (accessRight == null)
                return null;

            var hasAccess = await CheckAccess(accountId, accessRight.NodeId);
            if (!hasAccess)
                return null;

            var groupMember = await _context.AccessGroupMembers
                .FirstOrDefaultAsync(x => x.AccessGroupId == groupId && x.AccountId == userToRemove);

            if (groupMember == null)
                return null;

            _context.AccessGroupMembers.Remove(groupMember);
            await _context.SaveChangesAsync();

            return groupMember;
        }
    }
}