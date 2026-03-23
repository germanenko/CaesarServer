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
                //Cursor = cursor,
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

            var rule = await AddAccessRule(nodeBody.UpdatedBy, node);

            await _context.SaveChangesAsync();

            var body = result.ToNodeBody();
            body.AccessRight = rule.ToAccessRuleBody();

            return body;
        }

        public async Task<AccessRule> AddAccessRule(Guid accountId, Node node)
        {
            var subject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (subject == null)
            {
                subject = (await _context.UserAccessSubjects.AddAsync(new UserAccessSubject() { AccountId = accountId })).Entity;
            }

            var rule = (await _context.AccessRules.AddAsync(new AccessRule() { NodeId = node.Id, SubjectId = subject.Id, Permission = Permission.Write })).Entity;

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
                //Cursor = cursor
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

                if (nodeBody.Link == null) rule = (await AddAccessRule(nodeBody.UpdatedBy, node)).ToAccessRuleBody();

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

            foreach (var log in lastLogs)
            {
                var cache = await _context.SyncScopeAccess.FirstOrDefaultAsync(x => x.AccountId == accountId && x.ScopeId == log.ScopeId);

                var access = await CheckScopeAccess(accountId, log.ScopeId);
                Permission permission = Permission.Meta;

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

            await _context.SaveChangesAsync();
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


        public async Task<Node?> GetNode(Guid nodeId)
        {
            var node = await _context.Nodes.FirstOrDefaultAsync(x => x.Id == nodeId);

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
                //.Include(x => x.Cursor)
                .AsNoTracking()
                .ToListAsync();

            allNodes.AddRange(rootNodes);

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