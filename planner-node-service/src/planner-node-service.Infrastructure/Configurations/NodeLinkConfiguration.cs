using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class NodeLinkConfiguration : IEntityTypeConfiguration<NodeLink>
    {
        public void Configure(EntityTypeBuilder<NodeLink> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.ParentNode)
                .WithMany()
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.ChildNode)
                .WithMany()
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.RelationType)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(e => e.ParentId);
            builder.HasIndex(e => e.ChildId);
            builder.HasIndex(e => e.RelationType);

            builder.HasIndex(e => new { e.ParentId, e.ChildId, e.RelationType })
                .IsUnique();
        }
    }
}
