using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Infrastructure.Configurations
{
    public class WorkflowStatusConfiguration : IEntityTypeConfiguration<WorkflowStatusModel>
    {
        public void Configure(EntityTypeBuilder<WorkflowStatusModel> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Node)
                .WithMany()
                .HasForeignKey(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.NodeId);

            builder.HasIndex(e => new { e.NodeId, e.UpdatedAt })
                .IsUnique(false);
        }
    }
}