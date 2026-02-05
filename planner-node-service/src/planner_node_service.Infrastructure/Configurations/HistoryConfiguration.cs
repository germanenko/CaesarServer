using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class HistoryConfiguration : IEntityTypeConfiguration<History>
    {
        public void Configure(EntityTypeBuilder<History> builder)
        {
            builder.ToTable("History");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Action)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.TrackableId)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAddOrUpdate();

            builder.Property(e => e.UpdatedById)
                .IsRequired();

            builder.HasOne(e => e.Trackable)
                .WithMany()
                .HasForeignKey(e => e.TrackableId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.TrackableId)
                .HasDatabaseName("IX_Histories_TrackableId");

            builder.HasIndex(e => e.UpdatedById)
                .HasDatabaseName("IX_Histories_UpdatedById");
        }
    }
}