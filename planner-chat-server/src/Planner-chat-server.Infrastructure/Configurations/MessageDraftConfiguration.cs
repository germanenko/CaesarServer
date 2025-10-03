using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_chat_server.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Infrastructure.Configurations
{
    public class MessageDraftConfiguration : IEntityTypeConfiguration<MessageDraft>
    {
        public void Configure(EntityTypeBuilder<MessageDraft> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasOne(m => m.ChatMembership)
                .WithOne()
                .HasForeignKey<MessageDraft>(m => m.MembershipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => m.MembershipId)
                .IsUnique();  

            builder.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(4000);
        }
    }
}
