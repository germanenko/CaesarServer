using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_chat_server.Core.Entities.Models;

namespace Planner_chat_server.Infrastructure.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<FirebaseToken>
    {
        public void Configure(EntityTypeBuilder<FirebaseToken> builder)
        {
            builder.HasKey(e => new { e.UserId, e.Token });

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Token);

            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Token).IsRequired().HasMaxLength(500);
        }
    }
}
