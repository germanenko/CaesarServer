using Microsoft.EntityFrameworkCore;
using Planner_chat_server.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Infrastructure.Data
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
