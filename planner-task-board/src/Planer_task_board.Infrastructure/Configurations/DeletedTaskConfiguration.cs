using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class DeletedTaskConfiguration : IEntityTypeConfiguration<DeletedTask>
    {
        public void Configure(EntityTypeBuilder<DeletedTask> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ExistBeforeDate).IsRequired();
            builder.HasOne(e => e.Task)
                   .WithOne(e => e.DeletedTask);
        }
    }
}