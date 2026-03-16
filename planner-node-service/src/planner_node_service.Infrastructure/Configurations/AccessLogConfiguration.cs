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

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.HasOne(e => e.Node)
                .WithMany()
                .HasForeignKey(e => e.NodeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(e => e.GraphRevision)
                .IsRequired()
                .HasConversion<long>();

            builder.Property(e => e.RulesRevision)
                .IsRequired()
                .HasConversion<long>();

            builder.Property(e => e.Permission)
                .IsRequired()
                .HasConversion<int>();
        }
    }
}