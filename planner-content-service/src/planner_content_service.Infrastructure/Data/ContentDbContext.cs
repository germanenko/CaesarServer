using Microsoft.EntityFrameworkCore;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Infrastructure.Data
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

            modelBuilder.Entity<Node>().UseTptMappingStrategy();

            modelBuilder.Entity<Board>().HasBaseType<Node>();
            modelBuilder.Entity<Column>().HasBaseType<Node>();
            modelBuilder.Entity<TaskModel>().HasBaseType<Node>(); 
            modelBuilder.Entity<Chat>().HasBaseType<Node>();
            modelBuilder.Entity<ChatMessage>().HasBaseType<Node>();

            modelBuilder.Entity<Node>().ToTable("Nodes");
            modelBuilder.Entity<Board>().ToTable("Boards");
            modelBuilder.Entity<Column>().ToTable("Columns");
            modelBuilder.Entity<TaskModel>().ToTable("Tasks");
            modelBuilder.Entity<Chat>().ToTable("Chats");
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessages");
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
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
    }
}