using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_node_service.Core.Entities.Models;

namespace planner_node_service.Infrastructure.Configurations
{
    public class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
    {
        public void Configure(EntityTypeBuilder<AccessLog> builder)
        {
            builder.ToTable("AccessLogs");

            builder.HasKey(e => e.Seq);

            builder.Property(e => e.Seq)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.GraphRevision)
                .IsRequired()
                .HasConversion<long>();

            builder.Property(e => e.RulesRevision)
                .IsRequired()
                .HasConversion<long>();

            builder.Property(e => e.Permission)
                .IsRequired()
                .HasConversion<int>();

            builder.HasOne(e => e.Scope)
                .WithMany()
                .HasForeignKey(e => e.ScopeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}