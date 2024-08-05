using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_chat_server.Core.Entities.Models;

namespace Planner_chat_server.Infrastructure.Configurations
{
    public class ChatMembershipConfiguration : IEntityTypeConfiguration<ChatMembership>
    {
        public void Configure(EntityTypeBuilder<ChatMembership> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.AccountId);

            builder.HasOne(e => e.Chat)
                .WithMany(e => e.ChatMemberships)
                .HasForeignKey(e => e.ChatId);

            builder.HasMany(e => e.UserChatSessions)
                .WithOne(e => e.ChatMembership)
                .HasForeignKey(e => e.ChatMembershipId);
        }
    }
}