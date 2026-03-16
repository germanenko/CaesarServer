using Microsoft.EntityFrameworkCore;
using planner_node_service.Core.Entities.Models;
using System;
using System.Reflection;

namespace planner_node_service.Infrastructure.Data
{
    public class NodeDbContext : DbContext
    {
        public NodeDbContext(DbContextOptions<NodeDbContext> options) : base(options)
        {
        }

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
        public DbSet<TrackableEntity> Trackables { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<ContentLog> ContentLogs { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<TrackableEntity>().UseTptMappingStrategy();
            modelBuilder.Entity<Node>().HasBaseType<TrackableEntity>();

            modelBuilder.Entity<AccessSubject>().UseTptMappingStrategy();
            modelBuilder.Entity<UserAccessSubject>().HasBaseType<AccessSubject>();
            modelBuilder.Entity<GroupAccessSubject>().HasBaseType<AccessSubject>();
        }
    }
}
