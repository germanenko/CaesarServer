using Microsoft.EntityFrameworkCore;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Data
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
        }

        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<DeletedTask> DeletedTasks { get; set; }
        public DbSet<BoardColumn> BoardColumns { get; set; }
        public DbSet<BoardColumnTask> BoardColumnTasks { get; set; }
        public DbSet<BoardColumnMember> BoardColumnMembers { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<TaskPerformer> TaskPerformers { get; set; }
    }
}