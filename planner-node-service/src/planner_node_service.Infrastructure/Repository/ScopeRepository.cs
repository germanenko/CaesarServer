using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
using System.Security;

namespace planner_node_service.Infrastructure.Repository
{
    public class ScopeRepository : IScopeRepository
    {
        private readonly NodeDbContext _context;
        private readonly ILogger<AccessRepository> _logger;

        public ScopeRepository(NodeDbContext context, ILogger<AccessRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<SyncScopeAccess?> GetSyncScopeAccess(Guid accountId, Guid scopeId)
        {
            return await _context.SyncScopeAccess.FirstOrDefaultAsync(x => x.ScopeId == scopeId && x.AccountId == accountId);
        }


        public async Task<IEnumerable<Node>?> GetScopes(Guid accountId)
        {
            await ClearExcessSyncScopeAccess(accountId);

            var syncScopesAccess = await _context.SyncScopeAccess.Where(x => x.AccountId == accountId && x.Permission > Permission.None).ToListAsync();

            var scopeIds = syncScopesAccess.Select(x => x.ScopeId).ToList();

            var scopes = await _context.Nodes.Where(x => scopeIds.Contains(x.Id)).ToListAsync();

            return scopes;
        }

        public async Task<Node?> GetNodeScope(Guid nodeId)
        {
            var currentNodeId = nodeId;

            var currentNode = await _context.Nodes.FirstAsync(x => x.Id == nodeId);

            if (currentNode.SyncKind == SyncKind.Scope)
                return currentNode;

            while (currentNodeId != Guid.Empty)
            {
                var parent = (await _context.NodeLinks.Include(x => x.ParentNode).FirstOrDefaultAsync(x => x.ChildId == currentNodeId))?.ParentNode;

                if (parent == null)
                    break;

                if (parent.SyncKind == SyncKind.Scope)
                {
                    return parent;
                }

                currentNodeId = parent.Id;
            }

            return null;
        }

        public async Task<AccessRule?> CheckScopeAccess(Guid accountId, Guid scopeId)
        {
            var userSubject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (userSubject == null)
            {
                return null;
            }

            var scopeAccess = await _context.AccessRules.FirstOrDefaultAsync(x => x.NodeId == scopeId && x.SubjectId == userSubject.Id);
            if (scopeAccess != null)
            {
                return scopeAccess;
            }

            var currentNodeId = scopeId;

            var currentLevelIds = new HashSet<Guid> { scopeId };

            for (var level = 0; level < 5 && currentLevelIds.Any(); level++)
            {
                var links = await _context.NodeLinks
                    .Where(x => currentLevelIds.Contains(x.ParentId) && x.ParentId != x.ChildId)
                    .Include(x => x.ParentNode)
                    //.ThenInclude(x => x.Cursor)
                    .Include(x => x.ChildNode)
                    //.ThenInclude(x => x.Cursor)
                    .AsNoTracking()
                    .ToListAsync();

                var nextLevelIds = new HashSet<Guid>();
                foreach (var link in links)
                {
                    var access = await _context.AccessRules.FirstOrDefaultAsync(x => x.NodeId == link.ChildId && x.SubjectId == userSubject.Id);
                    if (access != null)
                    {
                        return access;
                    }

                    nextLevelIds.Add(link.ChildId);
                }

                currentLevelIds = nextLevelIds;
            }

            return null;
        }



        public async Task ClearExcessSyncScopeAccess(Guid accountId)
        {
            var userSubject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (userSubject == null)
            {
                return;
            }

            var rules = await _context.AccessRules.Include(x => x.Node).Where(x => x.SubjectId == userSubject.Id).ToListAsync();

            var ruleNodes = rules.Select(x => x.Node).ToList();

            var scopes = new List<Node>();

            foreach (var node in ruleNodes)
            {
                var scope = await GetNodeScope(node.Id);

                if (scope != null)
                    scopes.Add(scope);
            }

            var scopeIds = scopes.Select(x => x.Id).ToList();

            var lastLogs = await _context.AccessLogs
                .Where(x => scopeIds.Contains(x.ScopeId))
                .GroupBy(x => x.ScopeId)
                .Select(g => g.OrderByDescending(x => x.Seq).First())
                .ToListAsync();

            var excessSyncScopes = await _context.SyncScopeAccess.Where(x => x.AccountId == accountId && !scopeIds.Contains(x.ScopeId)).ToListAsync();

            _context.SyncScopeAccess.RemoveRange(excessSyncScopes);

            foreach (var scopeId in scopeIds)
            {
                var log = lastLogs.FirstOrDefault(x => x.ScopeId == scopeId);

                var access = await CheckScopeAccess(accountId, scopeId);
                Permission permission = Permission.Meta;

                var cache = await _context.SyncScopeAccess.FirstOrDefaultAsync(x => x.AccountId == accountId && x.ScopeId == scopeId);

                if (log != null)
                {
                    if (log.ScopeId == access!.NodeId)
                        permission = access.Permission;
                    else
                        permission = Permission.Meta;

                    if (cache != null)
                    {

                        if (cache.GraphRevisionUsed < log.GraphRevision ||
                            cache.RulesRevisionUsed < log.RulesRevision)
                        {
                            cache.Permission = permission;
                            cache.RulesRevisionUsed = log.RulesRevision;
                            cache.GraphRevisionUsed = log.GraphRevision;
                        }
                    }
                    else
                    {
                        await _context.SyncScopeAccess.AddAsync(new SyncScopeAccess()
                        {
                            AccountId = accountId,
                            ScopeId = log.ScopeId,
                            Permission = permission,
                            GraphRevisionUsed = log.GraphRevision,
                            RulesRevisionUsed = log.RulesRevision
                        });
                    }
                }
                else
                {
                    if (cache == null)
                    {
                        await _context.SyncScopeAccess.AddAsync(new SyncScopeAccess()
                        {
                            AccountId = accountId,
                            ScopeId = scopeId,
                            Permission = permission,
                            GraphRevisionUsed = 0,
                            RulesRevisionUsed = 0
                        });
                    }
                }
            }

            //foreach (var log in lastLogs)
            //{
            //    var cache = await _context.SyncScopeAccess.FirstOrDefaultAsync(x => x.AccountId == accountId && x.ScopeId == log.ScopeId);

            //    var access = await CheckScopeAccess(accountId, log.ScopeId);
            //    Permission permission = Permission.Meta;

            //    if (log.ScopeId == access!.NodeId)
            //        permission = access.Permission;
            //    else
            //        permission = Permission.Meta;

            //    if (cache != null)
            //    {

            //        if (cache.GraphRevisionUsed < log.GraphRevision ||
            //            cache.RulesRevisionUsed < log.RulesRevision)
            //        {
            //            cache.Permission = permission;
            //            cache.RulesRevisionUsed = log.RulesRevision;
            //            cache.GraphRevisionUsed = log.GraphRevision;
            //        }
            //    }
            //    else
            //    {
            //        await _context.SyncScopeAccess.AddAsync(new SyncScopeAccess()
            //        {
            //            AccountId = accountId,
            //            ScopeId = log.ScopeId,
            //            Permission = permission,
            //            GraphRevisionUsed = log.GraphRevision,
            //            RulesRevisionUsed = log.RulesRevision
            //        });
            //    }
            //}

            await _context.SaveChangesAsync();
        }

    }
}
