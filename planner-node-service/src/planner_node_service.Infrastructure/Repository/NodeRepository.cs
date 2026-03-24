using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;

namespace planner_node_service.Infrastructure.Repository
{
    public class NodeRepository : INodeRepository
    {
        private readonly NodeDbContext _context;
        private readonly ILogger<NodeRepository> _logger;
        private readonly IScopeRepository _scopeRepository;

        public NodeRepository(NodeDbContext context, ILogger<NodeRepository> logger, IScopeRepository scopeRepository)
        {
            _context = context;
            _logger = logger;
            _scopeRepository = scopeRepository;
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

            await AddContentLog(nodeBody.Id, nodeBody.Id);

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

            if (nodeBody.Link != null)
            {
                var scope = await _scopeRepository.GetNodeScope(nodeBody.Link.ParentId);

                if (scope != null)
                {
                    await AddContentLog(scope.Id, nodeBody.Id);
                }
            }

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

            var scope = await _scopeRepository.GetNodeScope(node.Id);

            if (scope != null)
            {
                await AddContentLog(scope.Id, nodeId, ActionType.Delete);
            }

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

        public async Task<IEnumerable<Node>?> GetNodes(Guid accountId, List<Guid>? rootIds = null)
        {
            IEnumerable<Node>? nodes = await GetNodesTree(accountId, rootIds);

            return nodes;
        }

        public async Task<NodeBody?> GetNodeParent(Guid nodeId)
        {
            var nodeParent = (await _context.NodeLinks.Include(x => x.ParentNode).FirstOrDefaultAsync(x => x.ChildId == nodeId))?.ParentNode;

            return nodeParent?.ToNodeBody();
        }

        public async Task<NodeLinkBody?> ChangeNodeParent(Guid accountId, Guid nodeId, Guid newParentId)
        {
            var userSubject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (userSubject == null)
            {
                return null;
            }

            var nodeLink = await _context.NodeLinks.FirstOrDefaultAsync(x => x.ChildId == nodeId);

            if (nodeLink == null)
            {
                return null;
            }

            var oldScope = await _scopeRepository.GetNodeScope(nodeId);
            var newScope = await _scopeRepository.GetNodeScope(newParentId);

            nodeLink.ParentId = newParentId;

            await AddAccessLog(userSubject.Id, newScope.Id);
            await AddContentLog(newScope.Id, nodeId);

            if (oldScope.Id != newScope.Id)
            {
                await AddAccessLog(userSubject.Id, oldScope.Id);
                await AddContentLog(oldScope.Id, nodeId);
            }

            await _context.SaveChangesAsync();

            return nodeLink?.ToBody();
        }

        public async Task AddAccessLog(Guid subjectId, Guid nodeId)
        {
            var lastLog = await _context.AccessLogs.AsNoTracking().OrderByDescending(x => x.Seq).FirstOrDefaultAsync(x => x.ScopeId == nodeId);

            var newLog = new AccessLog() { SubjectId = subjectId, ScopeId = nodeId, Permission = lastLog.Permission, GraphRevision = (lastLog?.GraphRevision ?? -1) + 1 };

            await _context.AccessLogs.AddAsync(newLog);
        }

        public async Task AddContentLog(Guid scopeId, Guid nodeId, ActionType? action = null)
        {
            var lastEntityLog = await _context.ContentLogs.AsNoTracking().OrderByDescending(x => x.Seq).FirstOrDefaultAsync(x => x.EntityId == nodeId);
            var lastScopeLog = await _context.ContentLogs.AsNoTracking().OrderByDescending(x => x.Seq).FirstOrDefaultAsync(x => x.ScopeId == scopeId);

            ActionType actionType;

            if (action == null)
            {
                actionType = lastEntityLog == null ? ActionType.Create : ActionType.Update;
            }
            else
            {
                actionType = action.Value;
            }

            var newLog = new ContentLog(scopeId, nodeId, actionType, (lastScopeLog?.ScopeVersion ?? -1) + 1);

            await _context.ContentLogs.AddAsync(newLog);
        }

        public async Task<Node?> GetNode(Guid nodeId)
        {
            var node = await _context.Nodes.FirstOrDefaultAsync(x => x.Id == nodeId);

            return node;
        }


        public async Task<IEnumerable<Node>?> GetNodesTree(Guid accountId, List<Guid>? rootNodeIds = null)
        {
            var userSubject = await _context.UserAccessSubjects.FirstOrDefaultAsync(x => x.AccountId == accountId);

            if (userSubject == null)
            {
                return null;
            }

            var query = _context.AccessRules
                .Where(x => x.SubjectId == userSubject.Id);

            if (rootNodeIds != null && rootNodeIds.Any())
                query = query.Where(x => rootNodeIds.Contains(x.NodeId));

            var rootIds = await query
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