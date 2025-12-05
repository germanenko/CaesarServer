using Microsoft.EntityFrameworkCore;
using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Enums;
using System;

namespace Planer_task_board.Infrastructure.Data
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);

            modelBuilder.Entity<Node>().UseTpcMappingStrategy();

            modelBuilder.Entity<Node>().ToTable("Nodes");
            modelBuilder.Entity<Board>().ToTable("Boards");
            modelBuilder.Entity<Column>().ToTable("Columns");
            modelBuilder.Entity<TaskModel>().ToTable("Tasks");
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeLink> NodeLinks { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<AccessRight> AccessRights { get; set; }
        public DbSet<AccessGroup> AccessGroups { get; set; }
        public DbSet<AccessGroupMember> AccessGroupMembers { get; set; }
        public DbSet<PublicationStatusModel> PublicationStatuses { get; set; }
        public DbSet<WorkflowStatusModel> WorkflowStatuses { get; set; }
        public DbSet<History> History { get; set; }
    }
}