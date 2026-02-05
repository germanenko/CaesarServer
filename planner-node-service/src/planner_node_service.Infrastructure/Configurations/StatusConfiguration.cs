using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class StatusConfiguration : IEntityTypeConfiguration<Status>
    {
        public void Configure(EntityTypeBuilder<Status> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Kind)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(e => new { e.NodeId, e.Kind })
                .IsUnique()
                .HasDatabaseName("IX_Status_NodeId_Kind");
        }
    }
}
