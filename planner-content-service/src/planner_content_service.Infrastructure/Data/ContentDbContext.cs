using Microsoft.EntityFrameworkCore;
using planner_content_service.Core.Entities.Models;
using planner_server_package.Idempotency;

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

            modelBuilder.Entity<Node>().ToTable("Nodes");
            modelBuilder.Entity<Board>().ToTable("Boards");
            modelBuilder.Entity<Column>().ToTable("Columns");
            modelBuilder.Entity<TaskModel>().ToTable("Tasks");
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
    }
}