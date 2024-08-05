using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planner_Auth.Core.Entities.Models;

namespace Planner_Auth.Infrastructure.Configurations
{
    public class AccountSessionConfiguration : IEntityTypeConfiguration<AccountSession>
    {
        public void Configure(EntityTypeBuilder<AccountSession> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.DeviceId)
                .IsRequired();

            builder.HasOne(s => s.Account)
                .WithMany(a => a.Sessions)
                .HasForeignKey(s => s.AccountId);
        }
    }
}