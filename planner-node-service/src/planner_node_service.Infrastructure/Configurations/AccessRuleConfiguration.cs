using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class AccessRuleConfiguration : IEntityTypeConfiguration<AccessRule>
    {
        public void Configure(EntityTypeBuilder<AccessRule> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(e => e.Node)
                .WithMany()
                .HasForeignKey(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.NodeId);
            builder.HasIndex(e => e.SubjectId);

            builder.Property(e => e.Permission)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(e => new { e.SubjectId, e.NodeId, e.Permission })
                .IsUnique();
        }
    }
}
