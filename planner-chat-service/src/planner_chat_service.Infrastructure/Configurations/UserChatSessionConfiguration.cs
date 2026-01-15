using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Infrastructure.Configurations
{
    public class UserChatSessionConfiguration : IEntityTypeConfiguration<AccountChatSession>
    {
        public void Configure(EntityTypeBuilder<AccountChatSession> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.SessionId);

            builder.HasOne(e => e.ChatSetting)
                .WithMany(e => e.UserChatSessions)
                .HasForeignKey(e => e.ChatSettingId);
        }
    }
}