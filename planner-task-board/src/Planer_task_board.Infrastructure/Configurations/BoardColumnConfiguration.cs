using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
    {
        public void Configure(EntityTypeBuilder<BoardColumn> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired();

            builder.HasMany(e => e.Members)
                .WithOne(e => e.Column)
                .HasForeignKey(e => e.ColumnId);

            builder.HasOne(e => e.Board)
                .WithMany(e => e.Columns)
                .HasForeignKey(e => e.BoardId);
        }
    }
}