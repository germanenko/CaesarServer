using Microsoft.EntityFrameworkCore;
using Npgsql;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;

namespace planner_node_service.Infrastructure.Repository
{
    public class AccessRepository : IAccessRepository
    {
        private readonly NodeDbContext _context;

        public AccessRepository(NodeDbContext context)
        {
            _context = context;
        }

        public async Task<SyncScopeAccess?> GetSyncScopeAccess(Guid accountId, Guid scopeId)
        {
            return await _context.SyncScopeAccess.FirstOrDefaultAsync(x => x.ScopeId == scopeId && x.AccountId == accountId);
        }

        public async Task<SyncScopeAccess?> CreateSyncScopeAccess(Guid accountId, Guid scopeId, Permission permission)
        {
            var scope = await GetSyncScopeAccess(accountId, scopeId);

            if (scope != null)
            {
                return scope;
            }

            var newScope = (await _context.SyncScopeAccess.AddAsync(new SyncScopeAccess() { AccountId = accountId, ScopeId = scopeId, Permission = permission })).Entity;

            await _context.SaveChangesAsync();

            return newScope;
        }

        public async Task<AccessRule?> ChangePermission(Guid granterId, Guid granteeId, Guid nodeId, Permission permission)
        {
            var userSubject = await CreateOrGetUserSubject(granteeId);

            var existing = await _context.AccessRules
                .FirstOrDefaultAsync(x =>
                    x.SubjectId == userSubject.Id &&
                    x.NodeId == nodeId);

            if (existing == null)
            {
                return null;
            }

            existing.Permission = permission;

            var scope = await _context.Nodes.FirstOrDefaultAsync(x => x.Id == nodeId && x.SyncKind == SyncKind.Scope);
            if (scope != null)
            {
                var syncScopeAsync = await _context.SyncScopeAccess.FirstOrDefaultAsync(x => x.ScopeId == scope.Id);

                if (syncScopeAsync != null)
                    syncScopeAsync.Permission = permission;
            }

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<AccessRule?> GrantAccess(Guid granterId, Guid granteeId, Guid nodeId, Permission permission)
        {
            var userSubject = await CreateOrGetUserSubject(granteeId);

            var existing = await _context.AccessRules
                .FirstOrDefaultAsync(x =>
                    x.SubjectId == userSubject.Id &&
                    x.NodeId == nodeId &&
                    x.Permission == permission);

            if (existing != null)
            {
                return existing;
            }

            var accessRight = new AccessRule
            {
                Id = Guid.NewGuid(),
                SubjectId = userSubject.Id,
                NodeId = nodeId,
                Permission = permission
            };

            _context.AccessRules.Add(accessRight);

            var scope = await _context.Nodes.FirstOrDefaultAsync(x => x.Id == nodeId && x.SyncKind == SyncKind.Scope);
            if (scope != null)
            {
                await _context.SyncScopeAccess.AddAsync(new SyncScopeAccess() { AccountId = granteeId, ScopeId = scope.Id, Permission = permission });
            }

            await _context.SaveChangesAsync();

            return accessRight;
        }

        public async Task<UserAccessSubject> CreateOrGetUserSubject(Guid accountId)
        {
            var userSubject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (userSubject == null)
            {
                userSubject = (await _context.UserAccessSubjects.AddAsync(new UserAccessSubject() { AccountId = accountId })).Entity;
            }

            return userSubject;
        }

        public async Task<bool> RevokeAccess(Guid granterId, Guid granteeId, Guid nodeId)
        {
            var userSubject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == granteeId);

            if (userSubject == null)
            {
                return false;
            }

            var existing = await _context.AccessRules
                .FirstOrDefaultAsync(x =>
                    x.SubjectId == userSubject.Id &&
                    x.NodeId == nodeId);

            if (existing != null)
            {
                _context.AccessRules.Remove(existing);

                await _context.AccessLogs.AddAsync(new AccessLog() { SubjectId = userSubject.Id, Permission = Permission.None, NodeId = nodeId });

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<GroupAccessSubject?> CreateGroup(Guid accountId, CreateAccessGroupBody body)
        {
            var hasAccess = await CheckAccess(accountId, body.BoardId, Permission.Write);
            if (!hasAccess)
                return null;

            var group = new GroupAccessSubject()
            {
                Id = Guid.NewGuid(),
                Name = body.Name
            };

            await _context.GroupAccessSubjects.AddAsync(group);

            var members = body.UserIds.Select(userId => new GroupMember()
            {
                GroupId = group.Id,
                AccountId = userId
            }).ToList();

            await _context.AccessGroupMembers.AddRangeAsync(members);

            var accessRight = new AccessRule()
            {
                NodeId = body.BoardId,
                SubjectId = group.Id
            };
            await _context.AccessRules.AddAsync(accessRight);

            await _context.SaveChangesAsync();

            group.Members = members;
            return group;
        }

        public async Task<GroupMember?> AddUserToGroup(Guid accountId, Guid userToAdd, Guid groupId)
        {
            var accessRight = await _context.AccessRules
                .FirstOrDefaultAsync(x => x.SubjectId == groupId);

            if (accessRight == null)
                return null;

            var hasAccess = await CheckAccess(accountId, accessRight.NodeId, Permission.Write);
            if (!hasAccess)
                return null;

            var existingMember = await _context.AccessGroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.AccountId == userToAdd);

            if (existingMember != null)
                return existingMember;

            var member = new GroupMember()
            {
                GroupId = groupId,
                AccountId = userToAdd
            };

            await _context.AccessGroupMembers.AddAsync(member);
            await _context.SaveChangesAsync();

            return member;
        }

        public async Task<bool> CheckAccess(Guid accountId, Guid nodeId, Permission minRequiredPermission)
        {
            bool access = false;

            var currentNodeId = nodeId;

            while (currentNodeId != Guid.Empty)
            {
                var subjectIds = await _context.AccessRules
                    .Where(ar => ar.NodeId == currentNodeId && ar.Permission >= minRequiredPermission)
                    .Select(x => x.SubjectId)
                    .ToListAsync();

                var directAccessSubjects = await _context.UserAccessSubjects
                    .Where(u => subjectIds.Contains(u.Id) && u.AccountId == accountId)
                    .Select(u => u.Id)
                    .ToListAsync();

                var groupAccessSubjects = await _context.GroupAccessSubjects
                    .Where(g => subjectIds.Contains(g.Id))
                    .SelectMany(g => g.Members)
                    .Where(m => m.AccountId == accountId)
                    .Select(m => m.GroupId)
                    .ToListAsync();

                var allAccessSubjectIds = directAccessSubjects
                    .Concat(groupAccessSubjects)
                    .Distinct()
                    .ToList();

                if (allAccessSubjectIds.Any()) return true;

                currentNodeId = await _context.NodeLinks
                    .Where(x => x.ChildId == currentNodeId && x.ParentId != x.ChildId)
                    .Select(x => x.ParentId)
                    .FirstOrDefaultAsync();
            }

            return access;
        }


        public async Task<GroupMember?> RemoveUserFromGroup(Guid accountId, Guid userToRemove, Guid groupId)
        {
            var accessRight = await _context.AccessRules
                .FirstOrDefaultAsync(x => x.SubjectId == groupId);

            if (accessRight == null)
                return null;

            var hasAccess = await CheckAccess(accountId, accessRight.NodeId, Permission.Write);
            if (!hasAccess)
                return null;

            var groupMember = await _context.AccessGroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.AccountId == userToRemove);

            if (groupMember == null)
                return null;

            _context.AccessGroupMembers.Remove(groupMember);
            await _context.SaveChangesAsync();

            return groupMember;
        }

        public async Task<AccessBody?> GetAccessRules(Guid accountId)
        {
            var userRules = _context.AccessRules
                .Where(ar =>
                    ar.Subject is UserAccessSubject &&
                    ((UserAccessSubject)ar.Subject).AccountId == accountId);

            var groupRules = _context.AccessRules
                .Where(ar =>
                    ar.Subject is GroupAccessSubject &&
                    ((GroupAccessSubject)ar.Subject)
                        .Members.Any(m => m.AccountId == accountId));

            var accessRights = await userRules
                .Union(groupRules)
                .ToListAsync();


            if (!accessRights.Any())
                return null;

            var accessBody = new AccessBody();

            accessRights = accessRights.DistinctBy(x => x.Id).ToList();

            accessBody.AccessRights = accessRights.Select(x => x.ToAccessRuleBody()).ToList();

            var accessGroups = groupRules
                .Select(x => x.Subject as GroupAccessSubject)
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