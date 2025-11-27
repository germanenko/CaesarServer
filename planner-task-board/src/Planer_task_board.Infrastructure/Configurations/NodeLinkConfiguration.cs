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
    public class NodeLinkConfiguration : IEntityTypeConfiguration<NodeLink>
    {
        public void Configure(EntityTypeBuilder<NodeLink> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.InternalId)
            .IsUnique();

            builder.Property(e => e.InternalId)
            .ValueGeneratedOnAdd();

            builder.HasOne(e => e.ParentNode)
                .WithMany()
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ChildNode)
                .WithMany()
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.RootId)
                .IsRequired();

            builder.Property(e => e.ChildType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(e => e.RelationType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.HasIndex(e => e.RootId);
            builder.HasIndex(e => e.ParentId);
            builder.HasIndex(e => e.ChildId);
            builder.HasIndex(e => e.ChildType);
            builder.HasIndex(e => e.RelationType);

            builder.HasIndex(e => new { e.ParentId, e.ChildId, e.RelationType })
                .IsUnique();
        }
    }
}
