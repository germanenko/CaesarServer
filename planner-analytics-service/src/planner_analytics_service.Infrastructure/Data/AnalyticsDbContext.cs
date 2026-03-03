using Microsoft.EntityFrameworkCore;
using planner_analytics_service.Core.Entities.Models;
using System.Reflection;

namespace planner_analytics_service.Infrastructure.Data
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
        {
        }

        public DbSet<AnalyticsAction> AnalyticsActions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
