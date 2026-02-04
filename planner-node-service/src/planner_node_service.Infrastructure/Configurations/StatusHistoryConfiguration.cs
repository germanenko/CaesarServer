using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class StatusHistoryConfiguration : IEntityTypeConfiguration<StatusHistory>
    {
        public void Configure(EntityTypeBuilder<StatusHistory> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Node)
                .WithMany()
                .HasForeignKey(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.NodeId);

            builder.HasOne(e => e.NewStatus)
                .WithMany()
                .HasForeignKey(e => e.NewStatusId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.NewStatusId);

            builder.HasOne(e => e.OldStatus)
                .WithMany()
                .HasForeignKey(e => e.OldStatusId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.OldStatusId);
        }
    }
}
