using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class BoardMemberConfiguration : IEntityTypeConfiguration<BoardMember>
    {
        public void Configure(EntityTypeBuilder<BoardMember> builder)
        {
            builder.HasKey(e => new { e.BoardId, e.AccountId });
            builder.HasOne(e => e.Board)
                .WithMany(e => e.Members)
                .HasForeignKey(e => e.BoardId);

            builder.Property(e => e.Role)
                .IsRequired();
        }
    }
}