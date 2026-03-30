using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Infrastructure.Configurations
{
    public class ChatUserStateConfiguration : IEntityTypeConfiguration<ChatUserState>
    {
        public void Configure(EntityTypeBuilder<ChatUserState> builder)
        {
            builder.ToTable("ChatUserStates");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Chat)
                .WithMany()
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}