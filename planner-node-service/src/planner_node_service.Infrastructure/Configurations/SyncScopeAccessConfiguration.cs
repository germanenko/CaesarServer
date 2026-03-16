using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class SyncScopeAccessConfiguration : IEntityTypeConfiguration<SyncScopeAccess>
    {
        public void Configure(EntityTypeBuilder<SyncScopeAccess> builder)
        {
            builder.HasIndex(e => new { e.ScopeId, e.AccountId })
                .IsUnique();
        }
    }
}
