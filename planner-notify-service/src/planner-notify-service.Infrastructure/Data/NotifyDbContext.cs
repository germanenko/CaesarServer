using Microsoft.EntityFrameworkCore;
using planner_notify_service.Core.Entities.Models;
using System.Reflection;

namespace planner_notify_service.Infrastructure.Data
{
    public class NotifyDbContext : DbContext
    {
        public NotifyDbContext(DbContextOptions<NotifyDbContext> options) : base(options)
        {
        }

        public DbSet<FirebaseToken> FirebaseTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
