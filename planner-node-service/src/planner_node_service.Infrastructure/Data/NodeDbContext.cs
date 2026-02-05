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
        public DbSet<AccessRight> AccessRights { get; set; }
        public DbSet<AccessGroup> AccessGroups { get; set; }
        public DbSet<AccessGroupMember> AccessGroupMembers { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<StatusHistory> StatusHistory { get; set; }
        public DbSet<TrackableEntity> Trackables { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<TrackableEntity>().UseTptMappingStrategy();
            modelBuilder.Entity<Node>().HasBaseType<TrackableEntity>();
            modelBuilder.Entity<AccessRight>().HasBaseType<TrackableEntity>();
            modelBuilder.Entity<AccessGroup>().HasBaseType<TrackableEntity>();
            modelBuilder.Entity<NodeLink>().HasBaseType<TrackableEntity>();
        }
    }
}
