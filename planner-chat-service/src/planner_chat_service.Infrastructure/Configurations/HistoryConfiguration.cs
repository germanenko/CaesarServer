using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Infrastructure.Configurations
{
    public class HistoryConfiguration : IEntityTypeConfiguration<History>
    {
        public void Configure(EntityTypeBuilder<History> builder)
        {
            builder.ToTable("Histories");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.NodeId)
                .IsRequired();

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

            builder.HasOne(e => e.Node)
                .WithMany()
                .HasForeignKey(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.NodeId)
                .HasDatabaseName("IX_Histories_NodeId");

            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Histories_CreatedAt");

            builder.HasIndex(e => e.CreatedBy)
                .HasDatabaseName("IX_Histories_CreatedBy");

            builder.HasIndex(e => e.UpdatedBy)
                .HasDatabaseName("IX_Histories_UpdatedBy");

            builder.HasIndex(e => new { e.NodeId, e.CreatedAt })
                .IsDescending(false, true) 
                .HasDatabaseName("IX_Histories_NodeId_CreatedAt");

            builder.HasIndex(e => new { e.CreatedAt, e.NodeId })
                .HasDatabaseName("IX_Histories_CreatedAt_NodeId");
        }
    }
}