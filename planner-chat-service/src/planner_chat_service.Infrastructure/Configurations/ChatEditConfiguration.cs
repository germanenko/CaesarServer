using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_chat_service.Core.Entities.Models;
namespace Planer_task_board.Infrastructure.Configurations
{
    public class ChatEditConfiguration : IEntityTypeConfiguration<ChatEdit>
    {
        public void Configure(EntityTypeBuilder<ChatEdit> builder)
        {
            builder.HasKey(e => e.Seq);

            builder.Property(e => e.Seq)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Action)
                .IsRequired()
                .HasConversion<int>();

            builder.HasOne(e => e.Chat)
                .WithMany()
                .HasForeignKey(e => e.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Message)
                .WithMany()
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
