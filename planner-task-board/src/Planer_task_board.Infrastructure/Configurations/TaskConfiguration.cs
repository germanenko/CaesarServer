using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<TaskModel>
    {
        public void Configure(EntityTypeBuilder<TaskModel> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(e => e.Description)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired();

            builder.Property(e => e.PriorityOrder)
                .IsRequired();

            builder.Property(e => e.HexColor)
                .HasMaxLength(7);

            builder.Property(e => e.Type)
                .IsRequired();

            builder.HasMany(e => e.Columns)
                .WithOne(e => e.Task)
                .HasForeignKey(e => e.TaskId);

            builder.HasOne(e => e.DeletedTask)
                .WithOne(e => e.Task)
                .HasForeignKey<DeletedTask>(e => e.TaskId);


        }
    }
}