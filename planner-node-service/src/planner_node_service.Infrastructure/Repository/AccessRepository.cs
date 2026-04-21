using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
using planner_server_package.Users;
using System.Data;
using System.Security;

namespace planner_node_service.Infrastructure.Repository
{
    public class AccessRepository : IAccessRepository
    {
        private readonly NodeDbContext _context;
        private readonly ILogger<AccessRepository> _logger;
        private readonly IScopeRepository _scopeRepository;
        private readonly IUserService _userService;

        public AccessRepository(NodeDbContext context, ILogger<AccessRepository> logger, IScopeRepository scopeRepository, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _scopeRepository = scopeRepository;
            _userService = userService;
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

            if (existing.Permission == permission)
            {
                return existing;
            }

            existing.Permission = permission;

            var scope = await _scopeRepository.GetNodeScope(nodeId);

            if (scope != null)
            {
                var logPermission = scope.Id == nodeId ? permission : Permission.Meta;
                await AddAccessLog(userSubject.Id, scope.Id, logPermission);
            }

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<AccessRule?> AddAccess(Guid accountId, Guid nodeId, Permission permission)
        {
            var userSubject = await CreateOrGetUserSubject(accountId);

            var existing = await _context.AccessRules
                .FirstOrDefaultAsync(x =>
                    x.SubjectId == userSubject.Id &&
                    x.NodeId == nodeId);

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

            var scope = await _scopeRepository.GetNodeScope(nodeId);

            if (scope != null)
            {
                Permission scopePermission;

                if (scope.Id == nodeId)
                {
                    scopePermission = permission;
                }
                else
                {
                    var scopeAccess = await _context.AccessRules
                        .FirstOrDefaultAsync(x => x.NodeId == scope.Id && x.SubjectId == userSubject.Id);

                    scopePermission = scopeAccess?.Permission ?? Permission.Meta;
                }

                await AddAccessLog(userSubject.Id, scope.Id, scopePermission);
            }

            await _context.SaveChangesAsync();

            return accessRight;
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

                await _context.SaveChangesAsync();

                var scope = await _scopeRepository.GetNodeScope(nodeId);

                if (scope != null)
                {
                    var access = await _scopeRepository.CheckScopeAccess(granteeId, scope.Id);
                    if (access != null)
                    {
                        await AddAccessLog(userSubject.Id, scope.Id, access.NodeId == scope.Id ? access.Permission : Permission.Meta);
                    }
                    else
                    {
                        await AddAccessLog(userSubject.Id, scope.Id, Permission.None);
                    }

                    await _context.SaveChangesAsync();

                }

                return true;
            }

            return false;
        }

        public async Task AddAccessLog(Guid subjectId, Guid nodeId, Permission permission)
        {
            var lastLog = await _context.AccessLogs.AsNoTracking().OrderByDescending(x => x.Seq).FirstOrDefaultAsync(x => x.ScopeId == nodeId);

            var newLog = new AccessLog() { SubjectId = subjectId, ScopeId = nodeId, Permission = permission, GraphRevision = lastLog?.GraphRevision ?? 0, RulesRevision = (lastLog?.RulesRevision ?? -1) + 1 };

            await _context.AccessLogs.AddAsync(newLog);
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
                var rules = await _context.AccessRules
                    .Include(x => x.Node)
                    .Where(ar => ar.NodeId == currentNodeId && ar.Permission >= minRequiredPermission)
                    .ToListAsync();

                var subjectIds = rules.Select(x => x.SubjectId);

                if (minRequiredPermission == Permission.Meta)
                {
                    foreach (var rule in rules)
                    {
                        if (rule.Node.SyncKind == SyncKind.Scope)
                        {
                            var scopeAccess = await _scopeRepository.CheckScopeAccess(accountId, rule.NodeId);

                            if (scopeAccess != null)
                            {
                                return true;
                            }
                        }
                    }
                }

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

        public async Task<List<AccessRuleBody>?> GetAccessRules(Guid accountId)
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

            accessRights = accessRights.DistinctBy(x => x.Id).ToList();

            var accessGroups = groupRules
                .Select(x => x.Subject as GroupAccessSubject)
                .Where(x => x != null)
                .Distinct()
                .ToList();

            var members = accessGroups
                .SelectMany(x => x.Members.Select(m => m.ToAccessGroupMemberBody()))
                .Distinct()
                .ToList();

            var ruleBodies = accessRights.Select(x => x.ToBody());

            foreach (var body in ruleBodies)
            {
                if (body.AccessSubject is UserAccessSubjectBody user)
                {
                    user.Profile = await _userService.GetUserData(user.AccountId);
                }
            }

            return ruleBodies.ToList();
        }

        public async Task<List<AccessRuleBody>?> GetCommonAccessRules(Guid accountId)
        {
            var userRules = _context.AccessRules
                .Include(x => x.Subject)
                .Where(ar =>
                    ar.Subject is UserAccessSubject &&
                    ((UserAccessSubject)ar.Subject).AccountId == accountId);

            var groupRules = _context.AccessRules
                .Include(x => x.Subject)
                .Where(ar =>
                    ar.Subject is GroupAccessSubject &&
                    ((GroupAccessSubject)ar.Subject)
                        .Members.Any(m => m.AccountId == accountId));

            var accessRights = await userRules
                .Union(groupRules)
                .ToListAsync();

            var nodeIds = accessRights
                .Select(x => x.NodeId)
                .Distinct()
                .ToList();

            var relatedRules = await _context.AccessRules
                .Include(x => x.Subject)
                .Where(ar => nodeIds.Contains(ar.NodeId))
                .ToListAsync();

            accessRights.AddRange(relatedRules);

            accessRights = accessRights.DistinctBy(x => x.Id).ToList();

            if (!accessRights.Any())
                return null;

            accessRights = accessRights.DistinctBy(x => x.Id).ToList();

            var accessGroups = groupRules
                .Select(x => x.Subject)
                .OfType<GroupAccessSubject>()
                .Distinct()
                .ToList();

            var members = accessGroups
                .SelectMany(x => x.Members.Select(m => m.ToAccessGroupMemberBody()))
                .Distinct()
                .ToList();

            var ruleBodies = accessRights.Select(x => x.ToBody()).ToList();

            var userIds = ruleBodies
                .Select(x => (x.AccessSubject as UserAccessSubjectBody)?.AccountId)
                .Where(x => x != null)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var users = await _userService.GetUsersData(userIds);

            var userMap = users.ToDictionary(x => x.Id);

            foreach (var body in ruleBodies)
            {
                if (body.AccessSubject is UserAccessSubjectBody user &&
                    userMap.TryGetValue(user.AccountId, out var profile))
                {
                    user.Profile = profile;
                }
            }

            return ruleBodies;
        }

        public async Task<AccessRule?> GetAccessRuleForNode(Guid accountId, Guid nodeId)
        {
            var userRules = _context.AccessRules
                .Where(ar =>
                    ar.Subject is UserAccessSubject &&
                    ((UserAccessSubject)ar.Subject).AccountId == accountId && ar.NodeId == nodeId);

            var groupRules = _context.AccessRules
                .Where(ar =>
                    ar.Subject is GroupAccessSubject &&
                    ((GroupAccessSubject)ar.Subject)
                        .Members.Any(m => m.AccountId == accountId && ar.NodeId == nodeId));

            var accessRights = await userRules
                .Union(groupRules)
                .ToListAsync();


            if (!accessRights.Any())
                return null;

            accessRights = accessRights.DistinctBy(x => x.Id).ToList();

            return accessRights.FirstOrDefault();
        }
    }
}