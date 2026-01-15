using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_chat_service.Core.Entities.Models;

namespace planner_chat_service.Infrastructure.Configurations
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