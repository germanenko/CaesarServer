using Microsoft.EntityFrameworkCore;
using planner_server_package.Idempotency.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency
{
    public class IdempotencyContext : DbContext, IIdempotencyContext
    {
        public IdempotencyContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdempotencyContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<ProcessOperation> ProcessedOperations { get; set; }
        public DbSet<OperationFailure> OperationFailures { get; set; }
    }
}
