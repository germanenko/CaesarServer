using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_chat_server.Core.Entities.Models;

namespace Planner_chat_server.Infrastructure.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.ToTable("Chats");

            builder.HasMany(e => e.ChatMemberships)
                .WithOne(e => e.Chat)
                .HasForeignKey(e => e.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}