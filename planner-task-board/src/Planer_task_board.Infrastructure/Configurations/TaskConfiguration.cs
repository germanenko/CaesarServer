using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
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

            builder.Property(t => t.PriorityOrder)
                .HasDefaultValue(0);

            builder.Property(t => t.HexColor)
                .HasMaxLength(7);

            builder.Property(t => t.CreatedAtDate)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(t => t.CreatedBy);
            builder.HasIndex(t => t.CreatorId);
            builder.HasIndex(t => t.PriorityOrder);
            builder.HasIndex(t => t.ChatId);
            builder.HasIndex(t => t.StartDate);
            builder.HasIndex(t => t.EndDate);
        }
    }
}
