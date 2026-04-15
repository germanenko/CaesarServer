using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_content_service.Core.Entities.Models;
using System.Text.Json;

namespace planner_content_service.Infrastructure.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Core.Entities.Models.Task>
    {
        public void Configure(EntityTypeBuilder<Core.Entities.Models.Task> builder)
        {
            var guidListComparer = new ValueComparer<List<Guid>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            builder.ToTable("Tasks");

            builder.Property(e => e.PerformerIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>(),
                    guidListComparer
                )
                .HasColumnType("jsonb");

        }
    }
}
