using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class BoardConfiguration : IEntityTypeConfiguration<Board>
    {
        public void Configure(EntityTypeBuilder<Board> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired();

            builder.HasMany(e => e.Columns)
                .WithOne(e => e.Board)
                .HasForeignKey(e => e.BoardId);

            builder.HasMany(e => e.Members)
                .WithOne(e => e.Board)
                .HasForeignKey(e => e.BoardId);
        }
    }
}