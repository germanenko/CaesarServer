using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency.Interface
{
    public interface IIdempotencyContext
    {
        public DbSet<ProcessOperation> ProcessedOperations { get; set; }
        public DbSet<OperationFailure> OperationFailures { get; set; }
    }
}
