using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_content_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Infrastructure.Configurations
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

            builder.HasIndex(e => new { e.AccountId, e.NodeId, e.AccessType })
                .IsUnique();
        }
    }
}
