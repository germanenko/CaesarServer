using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_chat_server.Core.Entities.Models;

namespace Planner_chat_server.Infrastructure.Configurations
{
    public class ChatSettingsConfiguration : IEntityTypeConfiguration<ChatSettings>
    {
        public void Configure(EntityTypeBuilder<ChatSettings> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasIndex(e => e.AccountId);

            builder.HasMany(e => e.UserChatSessions)
                .WithOne(e => e.ChatSetting)
                .HasForeignKey(e => e.ChatSettingId);
        }
    }
}