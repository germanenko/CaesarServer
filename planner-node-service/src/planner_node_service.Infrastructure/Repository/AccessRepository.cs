using Microsoft.EntityFrameworkCore;
using Npgsql;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
using planner_server_package.Entities;
using planner_server_package.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;
using static NpgsqlTypes.NpgsqlTsQuery;

namespace planner_node_service.Infrastructure.Repository
{
    public class AccessRepository : IAccessRepository
    {
        private readonly NodeDbContext _context;

        public AccessRepository(NodeDbContext context)
        {
            _context = context;
        }

        public async Task<AccessRight?> CreateAccessRight(Guid accountId, Guid nodeId, AccessType accessType)
        {
            var access = (await _context.AccessRights.AddAsync(new AccessRight()
            {
                AccountId = accountId,
                NodeId = nodeId,
                AccessType = accessType
            })).Entity;

            await _context.SaveChangesAsync();

            return access;
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
                AccountId = userId
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
                AccountId = userToAdd
            };

            await _context.AccessGroupMembers.AddAsync(member);
            await _context.SaveChangesAsync();

            return member;
        }

        public async Task<bool> CheckAccess(Guid accountId, Guid nodeId)
        {
            var isCreator = await _context.AccessRights
                .AnyAsync(n => n.Id == nodeId && n.AccessType == AccessType.Creator);

            if (isCreator)
                return true;

            return await _context.AccessRights
                .Where(ar => ar.NodeId == nodeId)
                .AnyAsync(ar =>
                    ar.AccessGroupId == null && ar.AccountId == accountId ||
                    ar.AccessGroupId != null && ar.AccessGroup.Members.Any(m => m.AccountId == accountId));
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

        public async Task<AccessBody?> GetAccessRights(Guid accountId)
        {
            var accessRights = await _context.AccessRights
                .Include(x => x.AccessGroup)
                    .ThenInclude(x => x.Members)
                .Where(ar =>
                    ar.AccessGroupId == null && ar.AccountId == accountId ||
                    ar.AccessGroupId != null &&
                     ar.AccessGroup != null &&
                     ar.AccessGroup.Members.Any(m => m.AccountId == accountId)
                )
                .ToListAsync();

            if (accessRights == null || !accessRights.Any())
                return null;

            var accessBody = new AccessBody();

            var accessNodes = accessRights.Select(x => x.NodeId);

            accessRights.AddRange(await _context.AccessRights.Where(x => accessNodes.Contains(x.NodeId)).ToListAsync());

            accessBody.AccessRights = accessRights.Select(x => x.ToAccessRightBody()).Distinct().ToList();

            var accessGroups = accessRights
                .Select(x => x.AccessGroup)
                .Where(x => x != null)
                .Distinct()
                .ToList();

            accessBody.AccessGroups = accessGroups
                .Select(x => x.ToAccessGroupBody())
                .ToList();

            var members = accessGroups
                .SelectMany(x => x.Members.Select(m => m.ToAccessGroupMemberBody()))
                .Distinct()
                .ToList();

            accessBody.AccessGroupMembers?.AddRange(members);

            return accessBody;
        }
    }
}