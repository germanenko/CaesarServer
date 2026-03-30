using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Infrastructure.Configurations
{
    public class ChatStateConfiguration : IEntityTypeConfiguration<ChatState>
    {
        public void Configure(EntityTypeBuilder<ChatState> builder)
        {
            builder.ToTable("ChatStates");

            builder.HasKey(x => x.ChatId);

            builder.HasOne(x => x.Chat)
                .WithOne()
                .HasForeignKey<ChatState>(x => x.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.EditCursor)
                .WithMany()
                .HasForeignKey(e => e.EditCursorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.ComplexProperty(x => x.LastPreview);
        }
    }
}