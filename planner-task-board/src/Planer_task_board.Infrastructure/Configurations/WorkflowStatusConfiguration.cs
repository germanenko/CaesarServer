using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class WorkflowStatusConfiguration : IEntityTypeConfiguration<WorkflowStatusModel>
    {
        public void Configure(EntityTypeBuilder<WorkflowStatusModel> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.InternalId)
            .IsUnique();

            builder.Property(e => e.InternalId)
            .ValueGeneratedOnAdd();

            builder.HasOne(e => e.Node)
                .WithMany()
                .HasForeignKey(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.NodeId);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasIndex(e => new { e.NodeId, e.UpdatedAt })
                .IsUnique(false);
        }
    }
}