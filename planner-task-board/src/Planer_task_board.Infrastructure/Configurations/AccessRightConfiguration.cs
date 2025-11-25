using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class AccessRightConfiguration : IEntityTypeConfiguration<AccessRight>
    {
        public void Configure(EntityTypeBuilder<AccessRight> builder)
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
            builder.HasIndex(e => e.AccountId);

            builder.Property(e => e.AccessType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(e => e.ResourceType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.HasIndex(e => new { e.AccountId, e.NodeId, e.AccessType })
                .IsUnique();
        }
    }
}
