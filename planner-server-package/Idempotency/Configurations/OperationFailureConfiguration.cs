using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace planner_server_package.Idempotency.Configurations
{
    public class OperationFailureConfiguration : IEntityTypeConfiguration<OperationFailure>
    {
        public void Configure(EntityTypeBuilder<OperationFailure> builder)
        {
            builder.HasKey(e => e.OperationId);

            builder.Property(e => e.ErrorCode)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.OperationName)
                .IsRequired()
                .HasConversion<int>();

        }
    }
}
