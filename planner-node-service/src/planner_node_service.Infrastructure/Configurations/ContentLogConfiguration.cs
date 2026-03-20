using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class ContentLogConfiguration : IEntityTypeConfiguration<ContentLog>
    {
        public void Configure(EntityTypeBuilder<ContentLog> builder)
        {
            builder.ToTable("ContentLogs");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ScopeVersion)
                .IsRequired()
                .HasConversion<long>();

            builder.Property(e => e.Action)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.EntityId)
                .IsRequired();

            builder.HasIndex(e => e.EntityId)
                .HasDatabaseName("IX_ContentLogs_EntityId");
        }
    }
}