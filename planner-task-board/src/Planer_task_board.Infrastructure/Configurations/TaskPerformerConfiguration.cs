using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class TaskPerformerConfiguration : IEntityTypeConfiguration<TaskPerformer>
    {
        public void Configure(EntityTypeBuilder<TaskPerformer> builder)
        {
            builder.HasKey(tp => new
            {
                tp.PerformerId,
                tp.TaskId
            });
        }
    }
}