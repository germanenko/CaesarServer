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
    public class NodeConfiguration : IEntityTypeConfiguration<Node>
    {
        public void Configure(EntityTypeBuilder<Node> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.InternalId)
                .IsUnique();

            builder.Property(e => e.InternalId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.Props)
                .HasColumnType("jsonb");

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAddOrUpdate();

            builder.Property(e => e.CreatedBy)
                .IsRequired();

            builder.Property(e => e.UpdatedBy)
                .IsRequired();

            builder.HasIndex(e => e.Type);
            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.CreatedBy);
            builder.HasIndex(e => e.CreatedAt);
            builder.HasIndex(e => e.UpdatedAt);

            builder.HasIndex(e => new { e.CreatedBy, e.CreatedAt });
        }
    }
}
