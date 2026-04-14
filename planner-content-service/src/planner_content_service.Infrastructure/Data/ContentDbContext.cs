using Microsoft.EntityFrameworkCore;
using planner_content_service.Core.Entities.Models;
using Task = planner_content_service.Core.Entities.Models.Task;

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
            modelBuilder.Entity<Task>().HasBaseType<Node>();

            modelBuilder.Entity<Node>().ToTable("Nodes");
            modelBuilder.Entity<Board>().ToTable("Boards");
            modelBuilder.Entity<Column>().ToTable("Columns");
            modelBuilder.Entity<Task>().ToTable("Tasks");
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Core.Entities.Models.Task> Tasks { get; set; }
    }
}