using Microsoft.EntityFrameworkCore;
using planner_node_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core
{
    public interface INodeDbContext
    {
        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeLink> NodeLinks { get; set; }
        public DbSet<SyncScopeAccess> SyncScopeAccess { get; set; }
        public DbSet<AccessRule> AccessRules { get; set; }
        public DbSet<AccessSubject> AccessSubjects { get; set; }
        public DbSet<GroupAccessSubject> GroupAccessSubjects { get; set; }
        public DbSet<UserAccessSubject> UserAccessSubjects { get; set; }
        public DbSet<GroupMember> AccessGroupMembers { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<StatusHistory> StatusHistory { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<ContentLog> ContentLogs { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
    }
}
