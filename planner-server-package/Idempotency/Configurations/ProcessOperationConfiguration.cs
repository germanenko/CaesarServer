using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace planner_server_package.Idempotency.Configurations
{
    public class ProcessOperationConfiguration : IEntityTypeConfiguration<ProcessOperation>
    {
        public void Configure(EntityTypeBuilder<ProcessOperation> builder)
        {
            builder.HasKey(e => e.OperationId);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.OperationName)
                .IsRequired()
                .HasConversion<int>();

        }
    }
}
