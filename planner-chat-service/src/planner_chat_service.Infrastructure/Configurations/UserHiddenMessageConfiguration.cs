using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using planner_chat_service.Core.Entities.Models;
namespace planner_chat_service.Infrastructure.Configurations
{
    public class UserHiddenMessageConfiguration : IEntityTypeConfiguration<UserHiddenMessage>
    {
        public void Configure(EntityTypeBuilder<UserHiddenMessage> builder)
        {
            builder.HasKey(e => new { e.MessageId, e.AccountId });

            builder.HasOne(e => e.Message)
                .WithMany()
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
