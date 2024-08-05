using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_chat_server.Core.Entities.Models;

namespace Planner_chat_server.Infrastructure.Configurations
{
    public class UserChatSessionConfiguration : IEntityTypeConfiguration<AccountChatSession>
    {
        public void Configure(EntityTypeBuilder<AccountChatSession> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.SessionId);

            builder.HasOne(e => e.ChatMembership)
                .WithMany(e => e.UserChatSessions)
                .HasForeignKey(e => e.ChatMembershipId);
        }
    }
}