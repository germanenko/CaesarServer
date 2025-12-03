using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Infrastructure.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.Property(cm => cm.MessageType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(cm => cm.Content)
                .HasColumnType("text") 
                .IsRequired();

            builder.Property(cm => cm.SentAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(cm => cm.SenderId)
                .IsRequired();

            builder.Property(cm => cm.HasBeenRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(cm => cm.SenderId)
                .HasDatabaseName("IX_ChatMessages_SenderId");

            builder.HasIndex(cm => cm.SentAt)
                .HasDatabaseName("IX_ChatMessages_SentAt")
                .IsDescending();

            builder.HasIndex(cm => cm.MessageType)
                .HasDatabaseName("IX_ChatMessages_MessageType");

            builder.HasIndex(cm => cm.HasBeenRead)
                .HasDatabaseName("IX_ChatMessages_HasBeenRead");

            builder.HasIndex(cm => new { cm.SenderId, cm.HasBeenRead, cm.SentAt })
                .HasDatabaseName("IX_ChatMessages_SenderId_HasBeenRead_SentAt");

            builder.Property(n => n.Name)
                .HasMaxLength(255)
                .HasDefaultValue("Chat Message")
                .IsRequired();

            builder.Property(n => n.Props)
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");
        }
    }
}