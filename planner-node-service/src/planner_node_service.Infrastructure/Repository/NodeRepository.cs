using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
using System.Security;
using static NpgsqlTypes.NpgsqlTsQuery;
using static System.Formats.Asn1.AsnWriter;

namespace planner_node_service.Infrastructure.Repository
{
    public class NodeRepository : INodeRepository
    {
        private readonly NodeDbContext _context;
        private readonly ILogger<NodeRepository> _logger;

        public NodeRepository(NodeDbContext context, ILogger<NodeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<NodeBody>> AddOrUpdateNodes(List<NodeBody> nodes)
        {
            List<NodeBody> newNodes = new List<NodeBody>();
            foreach (var node in nodes)
            {
                newNodes.Add(await AddOrUpdateNode(node));
            }
            return newNodes;
        }

        public async Task<NodeBody> AddScope(NodeBody nodeBody)
        {
            var existingScope = await _context.Nodes
                .FirstOrDefaultAsync(x => x.Id == nodeBody.Id);

            if (existingScope != null)
            {
                return existingScope.ToNodeBody();
            }

            var cursor = (await _context.ContentLogs.AddAsync(new ContentLog(nodeBody.Id, nodeBody.Id, ActionType.Create))).Entity;

            var node = new Node()
            {
                Id = nodeBody.Id,
                Name = nodeBody.Name,
                Type = nodeBody.Type,
                CursorId = cursor.Id,
                SyncKind = SyncKind.Scope
            };

            var result = (await _context.Nodes.AddAsync(node)).Entity;

            var nodeLink = new NodeLink
            {
                Id = Guid.NewGuid(),
                ParentId = node.Id,
                ChildId = node.Id,
                RelationType = RelationType.Me
            };

            await _context.History.AddAsync(new History() { Id = Guid.NewGuid(), UpdatedById = nodeBody.UpdatedBy, Action = ActionType.Create, NodeId = nodeBody.Id, UpdatedAt = nodeBody.UpdatedAt });

            var rule = await AddAccessRule(nodeBody);

            await _context.SaveChangesAsync();

            var body = result.ToNodeBody();
            body.AccessRight = rule.ToAccessRuleBody();

            return body;
        }

        public async Task<AccessRule> AddAccessRule(NodeBody nodeBody)
        {
            var subject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == nodeBody.UpdatedBy);
            if (subject == null)
            {
                subject = (await _context.UserAccessSubjects.AddAsync(new UserAccessSubject() { AccountId = nodeBody.UpdatedBy })).Entity;
            }

            var rule = (await _context.AccessRules.AddAsync(new AccessRule() { NodeId = nodeBody.Id, SubjectId = subject.Id, Permission = Permission.Write })).Entity;
            await _context.SyncScopeAccess.AddAsync(new SyncScopeAccess() { ScopeId = nodeBody.Id, AccountId = nodeBody.UpdatedBy, Permission = Permission.Write });

            await _context.AccessLogs.AddAsync(new AccessLog() { NodeId = nodeBody.Id, Permission = rule.Permission, SubjectId = rule.SubjectId });

            return rule;
        }

        public async Task<NodeBody> AddOrUpdateNode(NodeBody nodeBody)
        {
            var existingNode = await _context.Nodes
                .Where(x => x.Id == nodeBody.Id)
                .FirstOrDefaultAsync();

            var action = existingNode != null ? ActionType.Update : ActionType.Create;

            var cursor = (await _context.ContentLogs.AddAsync(new ContentLog(nodeBody.Id, nodeBody.Id, action))).Entity;

            var node = new Node()
            {
                Id = nodeBody.Id,
                Name = nodeBody.Name,
                Type = nodeBody.Type,
                CursorId = cursor.Id
            };

            if (existingNode == null)
            {
                var result = (await _context.Nodes.AddAsync(node)).Entity;

                var nodeLink = new NodeLink
                {
                    Id = Guid.NewGuid(),
                    ParentId = node.Id,
                    ChildId = node.Id,
                    RelationType = RelationType.Me
                };

                await _context.History.AddAsync(new History() { Id = Guid.NewGuid(), UpdatedById = nodeBody.UpdatedBy, Action = action, NodeId = nodeBody.Id, UpdatedAt = nodeBody.UpdatedAt });

                AccessRightBody rule = null;

                if (nodeBody.Link == null) rule = (await AddAccessRule(nodeBody)).ToAccessRuleBody();

                await _context.SaveChangesAsync();

                var body = result.ToNodeBody();
                body.AccessRight = rule;

                return body;
            }
            else
            {
                if (existingNode.Equals(node)) return existingNode.ToNodeBody();

                _context.Entry(existingNode).CurrentValues.SetValues(node);

                existingNode.Version++;

                await _context.SaveChangesAsync();

                return existingNode.ToNodeBody();
            }
        }

        public async Task<bool> DeleteNode(Guid accountId, Guid nodeId)
        {
            var node = await _context.Nodes.FirstOrDefaultAsync(n => n.Id == nodeId);

            if (node == null) return false;

            await _context.ContentLogs.AddAsync(new ContentLog(nodeId, nodeId, ActionType.Delete));

            _context.Remove(node);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task AddAccessLog(Guid subjectId, Guid nodeId, Permission permission)
        {
            var lastLog = await _context.AccessLogs.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.NodeId == nodeId);

            if (lastLog != null)
            {
                var newLog = new AccessLog() { SubjectId = subjectId, NodeId = nodeId, Permission = permission, GraphRevision = lastLog.GraphRevision++ };
                await _context.AccessLogs.AddAsync(newLog);
            }
        }

        public async Task<NodeLink> AddOrUpdateNodeLink(NodeLinkBody newNodeLink)
        {
            var existingNode = await _context.NodeLinks
                .Where(x => x.ChildId == newNodeLink.ChildId && x.ParentId == newNodeLink.ParentId)
                .FirstOrDefaultAsync();

            if (existingNode == null)
            {
                var newLink = new NodeLink
                {
                    Id = newNodeLink.Id,
                    ParentId = newNodeLink.ParentId,
                    ChildId = newNodeLink.ChildId,
                    RelationType = newNodeLink.RelationType
                };
                var result = await _context.NodeLinks.AddAsync(newLink);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            else
            {
                existingNode.ParentId = newNodeLink.ParentId;
                existingNode.ChildId = newNodeLink.ChildId;
                existingNode.RelationType = newNodeLink.RelationType;

                await _context.SaveChangesAsync();
                return existingNode;
            }
        }

        public async Task<List<NodeLink>> AddOrUpdateNodeLinks(List<NodeLinkBody> newNodeLinks)
        {
            var links = new List<NodeLink>();

            foreach (var link in newNodeLinks)
            {
                links.Add(await AddOrUpdateNodeLink(link));
            }

            return links;
        }

        public async Task<List<Guid>?> GetChildren(Guid parentId, RelationType? relationType = null)
        {
            var query = _context.NodeLinks.Where(x => x.ParentId == parentId);

            if (relationType.HasValue)
            {
                query = query.Where(x => x.RelationType == relationType.Value);
            }

            return await query.Select(x => x.ChildId).ToListAsync();
        }

        public async Task<IEnumerable<Node>?> GetNodes(Guid accountId)
        {
            IEnumerable<Node>? nodes = await GetNodesTree(accountId);

            return nodes;
        }


        public async Task<IEnumerable<Node>?> GetScopes(Guid accountId)
        {
            await ClearExcessSyncScopeAccess(accountId);

            var syncScopesAccess = await _context.SyncScopeAccess.Where(x => x.AccountId == accountId && x.Permission > Permission.None).ToListAsync();

            var scopeIds = syncScopesAccess.Select(x => x.ScopeId).ToList();

            var scopes = await _context.Nodes.Where(x => scopeIds.Contains(x.Id)).ToListAsync();

            return scopes;
        }

        public async Task ClearExcessSyncScopeAccess(Guid accountId)
        {
            var currentCache = await _context.SyncScopeAccess.Where(x => x.AccountId == accountId).ToListAsync();

            var scopeIds = currentCache.Select(x => x.ScopeId).ToList();

            var lastLogs = await _context.AccessLogs
                .Where(x => scopeIds.Contains(x.NodeId))
                .GroupBy(x => x.NodeId)
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .ToDictionaryAsync(x => x.NodeId, x => x);

            var excessSyncScopeAccess = new List<SyncScopeAccess>();

            foreach (var cache in currentCache)
            {
                if (lastLogs.TryGetValue(cache.ScopeId, out var lastLog))
                {
                    if (cache.GraphRevisionUsed < lastLog.GraphRevision ||
                        cache.RulesRevisionUsed < lastLog.RulesRevision)
                    {
                        if (await CheckScopeAccess(cache.AccountId, cache.ScopeId) == false)
                        {
                            excessSyncScopeAccess.Add(cache);
                        }
                        else
                        {
                            cache.RulesRevisionUsed = lastLog.RulesRevision;
                            cache.GraphRevisionUsed = lastLog.GraphRevision;
                            cache.Permission = lastLog.Permission;
                        }
                    }
                }
            }

            _context.SyncScopeAccess.RemoveRange(excessSyncScopeAccess);

            await _context.SaveChangesAsync();
        }

        public async Task ClearExcessSyncScopeAccess(Guid accountId, List<Guid> excessScopeIds)
        {
            var currentCache = await _context.SyncScopeAccess.Where(x => x.AccountId == accountId).ToListAsync();

            var excessScopesAccess = currentCache.Where(x => excessScopeIds.Contains(x.ScopeId)).ToList();

            _context.RemoveRange(excessScopesAccess);

            await _context.SaveChangesAsync();
        }


        public async Task<Node?> GetNode(Guid nodeId)
        {
            var node = await _context.Nodes.Include(x => x.Cursor).FirstOrDefaultAsync(x => x.Id == nodeId);

            return node;
        }


        public async Task<IEnumerable<Node>?> GetNodesTree(Guid accountId)
        {
            var userSubject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (userSubject == null)
            {
                return null;
            }

            var rootIds = await _context.AccessRules
                .Where(x => x.SubjectId == userSubject.Id)
                .Select(x => x.NodeId)
                .ToListAsync();

            if (!rootIds.Any())
                return Enumerable.Empty<Node>();

            var allNodeIds = new HashSet<Guid>(rootIds);
            var currentLevelIds = new HashSet<Guid>(rootIds);
            var allNodes = new List<Node>();

            var rootNodes = await _context.Nodes
                .Where(x => rootIds.Contains(x.Id))
                .Include(x => x.Cursor)
                .AsNoTracking()
                .ToListAsync();

            allNodes.AddRange(rootNodes);

            for (var level = 0; level < 5 && currentLevelIds.Any(); level++)
            {
                var links = await _context.NodeLinks
                    .Where(x => currentLevelIds.Contains(x.ParentId) && x.ParentId != x.ChildId)
                    .Include(x => x.ParentNode)
                        .ThenInclude(x => x.Cursor)
                    .Include(x => x.ChildNode)
                        .ThenInclude(x => x.Cursor)
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var link in links)
                {
                    allNodes.Add(link.ParentNode);
                    allNodes.Add(link.ChildNode);
                }

                var nextLevelIds = new HashSet<Guid>();
                foreach (var link in links)
                {
                    if (allNodeIds.Add(link.ChildId))
                    {
                        nextLevelIds.Add(link.ChildId);
                    }
                }

                currentLevelIds = nextLevelIds;
            }

            return allNodes.DistinctBy(x => x.Id).ToList();
        }

        public async Task<bool> CheckScopeAccess(Guid accountId, Guid scopeId)
        {
            var userSubject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (userSubject == null)
            {
                return false;
            }

            var currentNodeId = scopeId;

            var currentLevelIds = new HashSet<Guid> { scopeId };

            for (var level = 0; level < 5 && currentLevelIds.Any(); level++)
            {
                var links = await _context.NodeLinks
                    .Where(x => currentLevelIds.Contains(x.ParentId) && x.ParentId != x.ChildId)
                    .Include(x => x.ParentNode)
                        .ThenInclude(x => x.Cursor)
                    .Include(x => x.ChildNode)
                        .ThenInclude(x => x.Cursor)
                    .AsNoTracking()
                    .ToListAsync();

                var nextLevelIds = new HashSet<Guid>();
                foreach (var link in links)
                {
                    var access = await _context.AccessRules.FirstOrDefaultAsync(x => x.NodeId == link.ChildId && x.SubjectId == userSubject.Id);
                    if (access != null)
                    {
                        return true;
                    }

                    nextLevelIds.Add(link.ChildId);
                }

                currentLevelIds = nextLevelIds;
            }

            return false;
        }

        public async Task<IEnumerable<NodeLink>?> GetNodesLinks(Guid accountId)
        {
            var userSubject = await _context.Set<UserAccessSubject>()
                .FirstOrDefaultAsync(u => u.AccountId == accountId);

            var rootIds = await _context.AccessRules
                .Where(x => x.SubjectId == userSubject.Id)
                .Select(x => x.NodeId)
                .ToListAsync();

            if (!rootIds.Any())
                return Enumerable.Empty<NodeLink>();

            var allLinks = new List<NodeLink>();
            var visitedNodeIds = new HashSet<Guid>(rootIds);
            var currentLevelIds = new HashSet<Guid>(rootIds);

            for (var level = 0; level < 5 && currentLevelIds.Any(); level++)
            {
                var links = await _context.NodeLinks
                    .Where(x => currentLevelIds.Contains(x.ParentId) && x.ParentId != x.ChildId)
                    .AsNoTracking()
                    .ToListAsync();

                allLinks.AddRange(links);

                var nextLevelIds = new HashSet<Guid>();
                foreach (var link in links)
                {
                    if (visitedNodeIds.Add(link.ChildId))
                    {
                        nextLevelIds.Add(link.ChildId);
                    }
                }

                currentLevelIds = nextLevelIds;
            }

            return allLinks
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();
        }
    }
}