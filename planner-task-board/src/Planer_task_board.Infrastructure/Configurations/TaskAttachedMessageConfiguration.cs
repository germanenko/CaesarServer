using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class TaskAttachedMessageConfiguration : IEntityTypeConfiguration<TaskAttachedMessage>
    {
        public void Configure(EntityTypeBuilder<TaskAttachedMessage> builder)
        {
            builder.HasKey(e => new
            {
                e.TaskId,
                e.MessageId
            });
        }
    }
}