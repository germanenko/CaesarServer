using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planer_task_board.Core.Entities.Models;

namespace Planer_task_board.Infrastructure.Configurations
{
    public class BoardColumnMemberConfiguration : IEntityTypeConfiguration<BoardColumnMember>
    {
        public void Configure(EntityTypeBuilder<BoardColumnMember> builder)
        {
            builder.HasKey(e => new { e.ColumnId, e.AccountId });
            builder.HasOne(e => e.Column)
                .WithMany(e => e.Members)
                .HasForeignKey(e => e.ColumnId);
        }
    }
}