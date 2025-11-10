using Microsoft.EntityFrameworkCore;
using planner_notify_service.Core.Entities.Models;
using planner_notify_service.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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
