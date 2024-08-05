using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class BoardColumnTaskConfiguration : IEntityTypeConfiguration<BoardColumnTask>
    {
        public void Configure(EntityTypeBuilder<BoardColumnTask> builder)
        {
            builder.HasKey(e => new { e.ColumnId, e.TaskId });
            builder.HasOne(e => e.Column)
                .WithMany(e => e.Tasks)
                .HasForeignKey(e => e.ColumnId);

            builder.HasOne(e => e.Task)
                .WithMany(e => e.Columns)
                .HasForeignKey(e => e.TaskId);
        }
    }
}