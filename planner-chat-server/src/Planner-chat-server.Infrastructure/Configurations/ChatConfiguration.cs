using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_chat_server.Core.Entities.Models;

namespace Planner_chat_server.Infrastructure.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(c => c.Type)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasMany(e => e.Messages)
                .WithOne(e => e.Chat)
                .HasForeignKey(e => e.ChatId);

            builder.HasMany(e => e.ChatMemberships)
                .WithOne(e => e.Chat)
                .HasForeignKey(e => e.ChatId);
        }
    }
}