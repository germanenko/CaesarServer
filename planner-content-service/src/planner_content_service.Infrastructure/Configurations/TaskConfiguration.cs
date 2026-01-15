using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Infrastructure.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<TaskModel>
    {
        public void Configure(EntityTypeBuilder<TaskModel> builder)
        {
            builder.ToTable("Tasks");

            builder.Property(t => t.Name)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnType("text");

            builder.Property(t => t.HexColor)
                .HasMaxLength(7);

            builder.HasIndex(t => t.StartDate);
            builder.HasIndex(t => t.EndDate);
        }
    }
}
