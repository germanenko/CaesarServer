using Microsoft.EntityFrameworkCore;
using planner_node_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Infrastructure.Data
{
    public class NodeDbContext : DbContext
    {
        public NodeDbContext(DbContextOptions<NodeDbContext> options) : base(options)
        {
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeLink> NodeLinks { get; set; }
        public DbSet<AccessRight> AccessRights { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
