using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Infrastructure.Configurations
{
    public class AccessRightConfiguration : IEntityTypeConfiguration<AccessRight>
    {
        public void Configure(EntityTypeBuilder<AccessRight> builder)
        {
            builder.HasOne(e => e.Node)
                .WithMany() 
                .HasForeignKey(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.NodeId);
            builder.HasIndex(e => e.AccountId);

            builder.Property(e => e.AccessType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.NodeType)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(e => new { e.AccountId, e.NodeId, e.AccessType })
                .IsUnique();
        }
    }
}
